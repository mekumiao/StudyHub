using EntityFramework.Exceptions.Common;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;

namespace StudyHub.Service;

public class CourseCategoryService(StudyHubDbContext dbContext, IMapper mapper, ILogger<CourseCategoryService> logger, IServiceProvider serviceProvider)
    : CurdService<CourseCategory, int, CourseCategoryDto, CourseCategoryDto, CourseCategoryFilter, CourseCategoryCreate, CourseCategoryUpdate>(dbContext, mapper) {

    protected override IQueryable<CourseCategory> QueryAll() {
        return base.QueryAll().OrderBy(v => v.Order).ThenByDescending(v => v.CourseCategoryId);
    }

    public override async Task<ServiceResult> DeleteAsync(int id) {
        var exists = await _dbContext.Courses.AnyAsync(v => v.CourseCategoryId == id);
        return exists ? ServiceResult.Error("存在关联课程，请先删除相关课程后重试") : await base.DeleteAsync(id);
    }

    public async override Task<ServiceResult> DeleteItemsAsync(int[] ids) {
        var relatedItems = await _dbContext.Courses.Where(v => ids.Contains(v.CourseCategoryId)).Select(v => v.CourseCategoryId).ToArrayAsync();
        var dels = ids.Except(relatedItems).ToArray();
        try {
            await _dbContext.CourseCategories.Where(v => dels.Contains(v.CourseCategoryId)).ExecuteDeleteAsync();
        }
        catch (DbUpdateException ex) when (ex is DbUpdateConcurrencyException || ex is CannotInsertNullException) {
            logger.LogError(ex, "批量删除课程分类时出错 {ids}", ids);
            return ServiceResult.Error(ex.Message);
        }
        return ServiceResult.Ok();
    }

    public async override Task<ServiceResult<CourseCategoryDto>> CreateAsync(CourseCategoryCreate dto) {
        try {
            return await base.CreateAsync(dto);
        }
        catch (UniqueConstraintException ex) {
            logger.LogError(ex, "创建课程分类时异常 {dto}", dto);
            return ServiceResult.Error<CourseCategoryDto>("课程分类名称已存在");
        }
    }

    public async override Task<ServiceResult<CourseCategoryDto>> UpdateAsync(int id, CourseCategoryUpdate dto) {
        var category = await _dbContext.CourseCategories.FindAsync(id);
        if (category is null) {
            return ServiceResult.NotFound<CourseCategoryDto>();
        }
        var oldCategoryName = category.Name;

        await _dbContext.Entry(category).Collection(v => v.Courses).LoadAsync();

        _mapper.Map(dto, category);

        foreach (var item in category.Courses) {
            item.Category = category.Name;
        }

        try {
            await _dbContext.SaveChangesAsync();
        }
        catch (UniqueConstraintException ex) {
            logger.LogError(ex, "修改课程分类时异常 {dto}", dto);
            return ServiceResult.Error<CourseCategoryDto>("课程分类名称已存在");
        }

        if (string.IsNullOrWhiteSpace(dto.Name) is false) {
            var courseService = serviceProvider.GetRequiredService<CourseService>();
            courseService.EachUpdateReadme(yml => {
                if (yml.Category == oldCategoryName) {
                    yml.Category = dto.Name;
                }
            });
        }

        return ServiceResult.Ok(_mapper.Map<CourseCategoryDto>(category));
    }

    public async Task<ServiceResult> SortingAsync(IEnumerable<int> ids) {
        var categories = (from d in ids join c in _dbContext.CourseCategories on d equals c.CourseCategoryId select c).ToArray();
        int i = 1; // 从1开始，0留给新增项
        foreach (var item in categories) {
            item.Order = i++;
        }
        await _dbContext.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
