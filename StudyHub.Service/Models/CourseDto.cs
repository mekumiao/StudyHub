using StudyHub.Service.Base;
using StudyHub.Storage.Entities;

namespace StudyHub.Service.Models;

public class CourseFilter : IQueryableFilter<Course> {
    public int? CourseCategoryId { get; set; }
    public string? Keyword { get; set; }

    public IQueryable<Course> Build(IQueryable<Course> queryable) {
        if (CourseCategoryId.HasValue && CourseCategoryId.Value > 0) {
            queryable = queryable.Where(v => v.CourseCategoryId == CourseCategoryId);
        }
        if (string.IsNullOrWhiteSpace(Keyword) is false) {
            queryable = queryable.Where(v => v.Title.Contains(Keyword) || v.Description.Contains(Keyword));
        }
        return queryable;
    }
}

public class CourseDto {
    public int CourseId { get; set; }
    public string Cover { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SectionCount { get; set; }
    public int CourseCategoryId { get; set; }
    public IEnumerable<CourseSectionDto> CourseSections { get; set; } = [];
    public bool IsChecked { get; set; }
}

/// <summary>
/// 覆盖已有的课程信息
/// </summary>
public class CourseOverwrite {
    /// <summary>
    /// 本地文件图片全路径
    /// </summary>
    public string Cover { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    /// <summary>
    /// 课程目录，不能包含子路径
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;
    /// <summary>
    /// 分类名称
    /// </summary>
    public string Category { get; set; } = string.Empty;
    public CourseSectionCreate[] Sections { get; set; } = [];
}
