using EntityFramework.Exceptions.Common;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;

namespace StudyHub.Service;

public class TopicSubjectService(StudyHubDbContext dbContext, IMapper mapper, ILogger<TopicSubjectService> logger)
    : CurdService<TopicSubject, int, TopicSubjectDto, TopicSubjectDto, TopicSubjectFilter, TopicSubjectCreate, TopicSubjectUpdate>(dbContext, mapper) {
    private readonly StudyHubDbContext _dbContext = dbContext;

    protected override IQueryable<TopicSubject> QueryAll() {
        return base.QueryAll().OrderBy(v => v.TopicSubjectId);
    }

    public override async Task<ServiceResult> DeleteAsync(int id) {
        var existsTopic = await _dbContext.Topics.AnyAsync(v => v.TopicSubjectId == id);
        return existsTopic ? ServiceResult.Error("当前科目具有关联题目，请先删除相关题目后重试") : await base.DeleteAsync(id);
    }

    public override async Task<ServiceResult<TopicSubjectDto>> CreateAsync(TopicSubjectCreate dto) {
        try {
            return await base.CreateAsync(dto);
        }
        catch (UniqueConstraintException ex) {
            logger.LogError(ex, "创建科目时异常 {dto}", dto);
            return ServiceResult.Error<TopicSubjectDto>("科目名称已存在");
        }
    }

    public override async Task<ServiceResult<TopicSubjectDto>> UpdateAsync(int id, TopicSubjectUpdate dto) {
        try {
            return await base.UpdateAsync(id, dto);
        }
        catch (UniqueConstraintException ex) {
            logger.LogError(ex, "修改科目时异常 {dto}", dto);
            return ServiceResult.Error<TopicSubjectDto>("科目名称已存在");
        }
    }

    public override async Task<ServiceResult> DeleteItemsAsync(int[] ids) {
        var relatedItems = await _dbContext.Topics.Where(v => ids.Contains(v.TopicSubjectId)).Select(v => v.TopicSubjectId).ToArrayAsync();
        var dels = ids.Except(relatedItems).ToArray();
        try {
            await _dbContext.TopicSubjects.Where(v => dels.Contains(v.TopicSubjectId)).ExecuteDeleteAsync();
        }
        catch (DbUpdateException ex) when (ex is DbUpdateConcurrencyException || ex is CannotInsertNullException) {
            logger.LogError(ex, "批量删除科目时出错 {ids}", ids);
            return ServiceResult.Error(ex.Message);
        }
        return ServiceResult.Ok();
    }
}
