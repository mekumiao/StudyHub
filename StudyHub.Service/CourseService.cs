using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

using Mapster;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace StudyHub.Service;

public class CourseServiceRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<Course, CourseDto>();

        config.NewConfig<CourseSection, CourseSectionDto>();
    }
}

public class CourseService(
    ILogger<CourseService> logger,
    StudyHubDbContext dbContext,
    IHostApplicationLifetime applicationLifetime,
    IMapper mapper) {
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly static string DefaultCategory = "其他";
    public readonly static string CoursesRootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Courses");

    public async Task<ServiceResult<PagingResult<CourseDto>>> GetListAsync(CourseFilter filter, Paging paging) {
        var queryable = dbContext.Courses
            .AsNoTracking()
            .OrderByDescending(v => v.CourseId)
            .AsQueryable();
        queryable = filter.Build(queryable);
        var total = await queryable.CountAsync();
        queryable = paging.Build(queryable);
        var items = await queryable.ToArrayAsync();
        var resultItems = mapper.Map<CourseDto[]>(items);
        var pageResult = new PagingResult<CourseDto>(paging, total, resultItems);
        return ServiceResult.Ok(pageResult);
    }

    public async Task<ServiceResult<CourseDto>> GetEntityByIdAsync(int id) {
        var item = await dbContext.Courses
            .AsNoTracking()
            .Include(v => v.CourseCategory)
            .Include(v => v.CourseSections.OrderBy(s => s.Order))
            .SingleOrDefaultAsync(v => v.CourseId == id);
        if (item is null) {
            return ServiceResult.NotFound<CourseDto>();
        }
        return ServiceResult.Ok(mapper.Map<CourseDto>(item));
    }

    /// <summary>
    /// 修改课节的视频持续时长
    /// </summary>
    /// <param name="courseSectionId"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public async Task<ServiceResult> UpdateDurationAsync(int courseSectionId, TimeSpan duration) {
        var item = await dbContext.CourseSections
            .Include(v => v.Course)
            .SingleOrDefaultAsync(v => v.CourseSectionId == courseSectionId);
        if (item is null) {
            return ServiceResult.NotFound();
        }

        int coursePathLength = item.Course.RelativePath.Length + 1; // 排除掉斜杠
        var directoryInfo = new DirectoryInfo(Path.Combine(CoursesRootDirectory, item.Course.RelativePath));
        var readme = GetCourseByReadme(directoryInfo);
        if (readme is null) {
            return ServiceResult.NotFound();
        }

        static bool ArePathsEqual(string path1, string path2) {
            var fullPath1 = Path.GetFullPath(path1);
            var fullPath2 = Path.GetFullPath(path2);

            return string.Equals(fullPath1, fullPath2, StringComparison.OrdinalIgnoreCase);
        }

        var section = readme.Sections.FirstOrDefault(v => ArePathsEqual(CombineRelativePath(v.File, () => Path.Combine(CoursesRootDirectory, item.Course.RelativePath, v.File)), item.RelativePath));
        if (section is null) {
            return ServiceResult.NotFound();
        }

        section.Duration = duration;
        item.Duration = duration;
        SetCourseToReadme(directoryInfo, readme);
        await dbContext.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    /// <summary>
    /// 同步课程根目录，并保存到数据库
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public async Task<ServiceResult> SynchronizeCourseToDbFromRootDirectoryAsync() {
        var courseYmls = SynchronizeCourseFromRootDirectory().Where(v => v.Deleted == false).ToList();
        var newCategories = courseYmls.DistinctBy(v => v.Category).Select(v => new CourseCategory { Name = v.Category }).ToArray();
        var courses = new List<Course>();
        foreach (var item in courseYmls) {
            var course = mapper.Map<Course>(item);
            foreach (var section in item.Sections) {
                var courseSection = mapper.Map<CourseSection>(section);
                courseSection.RelativePath = CombineRelativePath(item.RelativePath, () => Path.Combine(CoursesRootDirectory, item.RelativePath, section.File));
                course.CourseSections.Add(courseSection);
                course.SectionCount += 1;
            }
            course.Cover = CombineRelativePath(item.Cover, () => Path.Combine(CoursesRootDirectory, item.RelativePath, item.Cover));
            courses.Add(course);
        }

        //using var scope = scopeFactory.CreateAsyncScope();
        //using var dbContext = scope.ServiceProvider.GetRequiredService<StudyHubDbContext>();

        var oldCategories = await dbContext.CourseCategories.ToArrayAsync();
        var categories = oldCategories.UnionBy(newCategories, v => v.Name).ToArray();
        var values = (from u in courses
                      join c in categories on u.Category equals c.Name
                      select new {
                          course = u,
                          category = c,
                      });
        foreach (var item in values) {
            item.course.CourseCategory = item.category;
        }

        try {
            using var trans = await dbContext.Database.BeginTransactionAsync();
            await dbContext.Courses.Where(v => true).ExecuteDeleteAsync(applicationLifetime.ApplicationStopping);
            await dbContext.Courses.AddRangeAsync(courses);
            await dbContext.SaveChangesAsync();
            await trans.CommitAsync();
            return ServiceResult.Ok();
        }
        catch (DbException ex) {
            logger.LogError(ex, "将课程同步保存到数据库时失败");
            return ServiceResult.Error(ex.Message);
        }
    }

    /// <summary>
    /// 将RelativePath拼接完整
    /// </summary>
    /// <param name="courseYml"></param>
    /// <returns></returns>
    public static string CombineRelativePath(string relativePath, Func<string> combine) {
        return string.IsNullOrWhiteSpace(relativePath)
            ? string.Empty : IsWebUrl(relativePath)
            ? relativePath
            : Path.IsPathRooted(relativePath) ? relativePath : combine();
    }

    private static bool IsWebUrl(string url) {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 同步课程根目录。
    /// </summary>
    /// <remarks>如果没有README.md文件则新建。从README.md中排除已删除的文件信息，附加新增的文件信息到README.md</remarks>
    /// <returns></returns>
    private List<CourseYmlModel> SynchronizeCourseFromRootDirectory() {
        var directoryInfo = new DirectoryInfo(CoursesRootDirectory);
        if (directoryInfo.Exists is false) return [];
        var courseYmls = new List<CourseYmlModel>();
        foreach (var dir in directoryInfo.GetDirectories()) {
            var info = SynchronizeCourseFromDirectory(dir);
            courseYmls.Add(info);
        }
        return courseYmls;
    }

    /// <summary>
    /// 将课程信息覆盖到README.yml并且同步到数据库
    /// </summary>
    /// <param name="course"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<ServiceResult> CourseOverwriteAsync(CourseOverwrite course) {
        if (string.IsNullOrWhiteSpace(course.RelativePath)) throw new ArgumentException("课程目录不能为空", nameof(course));

        var directoryInfo = new DirectoryInfo(Path.Combine(CoursesRootDirectory, course.RelativePath));
        try {
            if (directoryInfo.Exists is false) {
                return ServiceResult.NotFound($"未找到课程文件见 {course.RelativePath}");
            }
            var yml = SynchronizeCourseFromDirectory(directoryInfo);
            mapper
                .From(course)
                .ForkConfig(c => c.NewConfig<CourseOverwrite, CourseYmlModel>().Ignore(dest => dest.Sections))
                .AdaptTo(yml);

            int o = 1;
            var courseDir = Path.Combine(CoursesRootDirectory, course.RelativePath);
            foreach (var item in course.Sections) {
                item.Order = o++;
                if (Path.IsPathRooted(item.RelativePath)) {
                    item.RelativePath = item.RelativePath.Replace(courseDir, "").TrimStart('\\').TrimStart('/');
                }
            }

            var items = (from i in course.Sections
                         join j in yml.Sections on i.RelativePath equals j.File
                         select new { src = i, dest = j, });

            foreach (var it in items) {
                mapper.Map(it.src, it.dest);
            }

            yml.Sections = yml.Sections.OrderBy(v => v.Order);

            SetCourseToReadme(directoryInfo, yml);
            await SynchronizeCourseToDbFromRootDirectoryAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex) {
            logger.LogError(ex, "创建课程时发生错误");
            return ServiceResult.Error(ex.Message);
        }
    }

    /// <summary>
    /// 同步指定课程目录。
    /// </summary>
    /// <remarks>如果没有README.md文件则新建。从README.md中排除已删除的文件信息，附加新增的文件信息到README.md</remarks>
    /// <param name="directoryName">课程目录</param>
    /// <returns></returns>
    private CourseYmlModel SynchronizeCourseFromDirectory(DirectoryInfo directoryInfo) {
        if (directoryInfo.Exists is false) throw new ArgumentException("课程目录不存在", nameof(directoryInfo));

        var info = GetCourseByReadme(directoryInfo);
        var items = GetCourseSectionsByDirectory(directoryInfo);
        if (info is null) {
            info = new CourseYmlModel {
                Title = directoryInfo.Name,
                Description = directoryInfo.Name,
                Sections = items,
                Category = DefaultCategory,
            };
        }
        else {
            if (info.Sections.Any()) {
                // 让新增的文件排到后面
                int order = info.Sections.Last().Order + 1;
                foreach (var item in items) {
                    item.Order = order;
                }
            }
            info.Category = string.IsNullOrWhiteSpace(info.Category) ? DefaultCategory : info.Category.Trim();
            var outputQueryable = from t in items
                                  join f in info.Sections on t.File equals f.File into gj
                                  from subpet in gj.DefaultIfEmpty()
                                  select new CourseSectionYmlModel {
                                      Title = subpet?.Title ?? t.Title,
                                      Description = subpet?.Description ?? t.Description,
                                      File = subpet?.File ?? t.File,
                                      Duration = subpet?.Duration ?? t.Duration,
                                      Order = subpet?.Order ?? t.Order,
                                  };
            info.Sections = [.. outputQueryable.OrderBy(v => v.Order)];
        }

        info.RelativePath = directoryInfo.Name;
        SetCourseToReadme(directoryInfo, info);

        return info;
    }

    private static List<CourseSectionYmlModel> GetCourseSectionsByDirectory(DirectoryInfo directoryInfo) {
        if (directoryInfo.Exists is false) return [];
        var sections = new List<CourseSectionYmlModel>();
        var medias = new List<FileInfo>();
        medias.AddRange(directoryInfo.GetFiles("*.ts", SearchOption.TopDirectoryOnly));
        medias.AddRange(directoryInfo.GetFiles("*.mp4", SearchOption.TopDirectoryOnly));
        medias.AddRange(directoryInfo.GetFiles("*.avi", SearchOption.TopDirectoryOnly));
        medias.AddRange(directoryInfo.GetFiles("*.mkv", SearchOption.TopDirectoryOnly));
        medias.AddRange(directoryInfo.GetFiles("*.mov", SearchOption.TopDirectoryOnly));
        medias.AddRange(directoryInfo.GetFiles("*.wmv", SearchOption.TopDirectoryOnly));
        foreach (var item in medias.OrderBy(v => v.Name)) {
            var name = Path.GetFileNameWithoutExtension(item.Name);
            sections.Add(new CourseSectionYmlModel {
                Title = name,
                Description = name,
                File = item.Name,
            });
        }
        return sections;
    }

    private CourseYmlModel? GetCourseByReadme(DirectoryInfo directoryInfo) {
        var readmePath = Path.Combine(directoryInfo.FullName, "README.yml");
        if (Path.Exists(readmePath) is false) return null;
        var yml = File.ReadAllText(readmePath);
        var model = _deserializer.Deserialize<CourseYmlModel>(yml);
        int i = 1;
        foreach (var section in model.Sections) {
            section.Order = i++;
        }
        return model;
    }

    private void SetCourseToReadme(DirectoryInfo directoryInfo, CourseYmlModel dto) {
        var readmePath = Path.Combine(directoryInfo.FullName, "README.yml");
        var yml = _serializer.Serialize(dto);
        File.WriteAllText(readmePath, yml);
    }

    private static string GetFullPathOfCourse(Course course) {
        return Path.Combine(CoursesRootDirectory, course.RelativePath);
    }

    private static DirectoryInfo GetDirectoryInfoOfCourse(Course course) {
        return new DirectoryInfo(GetFullPathOfCourse(course));
    }

    private bool TryDeleteCourseDirectory(Course course, bool deleteFile, [NotNullWhen(false)] out string? message) {
        var directoryInfo = GetDirectoryInfoOfCourse(course);
        if (directoryInfo.Exists) {
            if (deleteFile) {
                try {
                    directoryInfo.Delete(true);
                }
                catch (IOException ex) {
                    message = ex.Message;
                    return false;
                }
            }
            else {
                var yml = GetCourseByReadme(directoryInfo);
                if (yml is not null) {
                    yml.Deleted = true;
                    SetCourseToReadme(directoryInfo, yml);
                }
            }
        }
        message = null;
        return true;
    }

    public async Task<ServiceResult> DeleteAsync(int courseId, bool deleteFile) {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null) {
            return ServiceResult.NotFound();
        }
        if (TryDeleteCourseDirectory(course, deleteFile, out var message) is false) {
            return ServiceResult.Error(message);
        }
        dbContext.Courses.Remove(course);
        await dbContext.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteItemsAsync(int[] courseIds, bool deleteFile) {
        var courses = await dbContext.Courses.Where(v => courseIds.Contains(v.CourseId)).ToArrayAsync();
        if (courses.Length == 0) {
            return ServiceResult.NotFound();
        }
        foreach (var course in courses) {
            if (TryDeleteCourseDirectory(course, deleteFile, out _)) {
                dbContext.Courses.Remove(course);
            }
        }
        await dbContext.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
