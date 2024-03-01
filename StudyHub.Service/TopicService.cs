using System.Diagnostics;

using EntityFramework.Exceptions.Common;

using Mapster;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;

namespace StudyHub.Service;

public class TopicServiceRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<Topic, TopicDto>()
            .Map(dest => dest.TopicSubjectName, src => src.TopicSubject.Name, should => should.TopicSubject != null);
    }
}

public class TopicService(StudyHubDbContext dbContext, IMapper mapper, ILogger<TopicService> logger)
    : ICurdService<Topic, int, TopicDto, TopicDto, TopicFilter, TopicCreate, TopicUpdate> {

    public async Task<ServiceResult<PagingResult<TopicDto>>> GetListAsync(TopicFilter filter, Paging paging) {
        var queryable = dbContext.Topics
            .AsNoTracking()
            .Include(v => v.TopicSubject)
            .Include(v => v.TopicOptions.OrderBy(t => t.Code))
            .OrderByDescending(v => v.TopicId)
            .AsQueryable();
        queryable = filter.Build(queryable);
        queryable = paging.Build(queryable);

        var totalQueryable = dbContext.Topics.AsNoTracking();
        totalQueryable = filter.Build(totalQueryable);
        var total = await totalQueryable.CountAsync();

        var result = await queryable.ToListAsync();
        var resultItems = mapper.Map<TopicDto[]>(result);
        var pageResult = new PagingResult<TopicDto>(paging, total, resultItems);
        return ServiceResult.Ok(pageResult);
    }

    public async Task<ServiceResult<TopicDto>> GetEntityByIdAsync(int id) {
        var item = await dbContext.Topics
            .AsNoTracking()
            .Include(v => v.TopicOptions.OrderBy(t => t.Code))
            .SingleOrDefaultAsync(v => v.TopicId == id);
        if (item == null) {
            return ServiceResult.NotFound<TopicDto>();
        }
        var result = mapper.Map<TopicDto>(item);
        return ServiceResult.Ok(result);
    }

    public async Task<ServiceResult<TopicDto>> CreateAsync(TopicCreate dto) {
        var item = mapper.Map<Topic>(dto);
        if (dto.TopicOptions.Any()) {
            item.TopicOptions.AddRange(mapper.Map<List<TopicOption>>(dto.TopicOptions));
        }
        dbContext.Topics.Add(item);
        await dbContext.SaveChangesAsync();
        var result = mapper.Map<TopicDto>(item);
        return ServiceResult.Ok(result);
    }

    public async Task<ServiceResult<TopicDto>> UpdateAsync(int id, TopicUpdate dto) {
        var item = await dbContext.Topics
            .Include(v => v.TopicOptions.OrderBy(t => t.Code))
            .SingleOrDefaultAsync(v => v.TopicId == id);
        if (item is null) {
            return ServiceResult.NotFound<TopicDto>();
        }
        mapper.Map(dto, item);
        if (dto.TopicOptions is not null) {
            item.TopicOptions.Clear();
            item.TopicOptions.AddRange(mapper.Map<List<TopicOption>>(dto.TopicOptions));
        }
        await dbContext.SaveChangesAsync();
        var result = mapper.Map<TopicDto>(item);
        return ServiceResult.Ok(result);
    }

    public async Task<ServiceResult> DeleteAsync(int id) {
        try {
            var item = new Topic { TopicId = id };
            dbContext.Topics.Attach(item);
            dbContext.Topics.Remove(item);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex) {
            logger.LogError(ex, "删除{topicid}时异常", id);
            return ServiceResult.Error(ex.Message);
        }
        catch (CannotInsertNullException ex) {
            logger.LogError(ex, "删除{topicid}时异常", id);
            return ServiceResult.Error("该题目已被使用，请先清空作答记录然后重试！");
        }
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ImportTopicFromExcelFileNameAsync(string fileName, TopicBankFlag topicBank) {
        using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return await ImportTopicFromExcelStreamAsync(stream, topicBank);
    }

    public async Task<ServiceResult> ImportTopicFromExcelStreamAsync(Stream stream, TopicBankFlag topicBank) {
        var translator = new ExcelTopicTranslator();
        var topics = translator.ParseExcelToTopics(stream);
        if (translator.IsSuccess is false) {
            return ServiceResult.Error(translator.GetErrors());
        }

        foreach (var item in topics) {
            item.TopicBankFlag = topicBank;
        }

        var newSubjects = topics.DistinctBy(v => v.SubjectName).Select(v => new TopicSubject { Name = v.SubjectName }).ToArray();

        var oldSubjects = await dbContext.TopicSubjects.ToArrayAsync();
        var subjects = oldSubjects.UnionBy(newSubjects, v => v.Name);
        var topics_upd = (from t in topics
                          join s in subjects on t.SubjectName equals s.Name
                          select new {
                              topic = t,
                              subject = s,
                          });
        foreach (var item in topics_upd) {
            item.topic.TopicSubject = item.subject;
        }

        try {
            await dbContext.Topics.AddRangeAsync(topics);
            await dbContext.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex) {
            Debug.Assert(false);
            logger.LogError(ex, "导入试卷：解析成功，保存到数据库失败，请重试");
            throw;
        }
    }

    public async Task<ServiceResult> DeleteItemsAsync(int[] ids) {
        var relatedItems = await dbContext.AnswerRecordItems.Where(v => ids.Contains(v.TopicId)).Select(v => v.TopicId).ToArrayAsync();
        var dels = ids.Except(relatedItems).ToArray();
        try {
            await dbContext.Topics.Where(v => dels.Contains(v.TopicId)).ExecuteDeleteAsync();
        }
        catch (DbUpdateException ex) when (ex is DbUpdateConcurrencyException || ex is CannotInsertNullException) {
            logger.LogError(ex, "批量删除题目时出错 {ids}", ids);
            return ServiceResult.Error(ex.Message);
        }
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ExportExcelFromTopicIdsAsync(string fileName, int[] topicIds) {
        var stream = default(FileStream);
        try {
            stream = File.Open(fileName, FileMode.Create, FileAccess.Write);
        }
        catch (DirectoryNotFoundException ex) {
            logger.LogError(ex, "无法创建导出文件，选择的文件夹不存在或已被删除");
            return ServiceResult.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex) {
            logger.LogError(ex, "无法创建导出文件，程序没有导出路径的访问权限");
            return ServiceResult.Error(ex.Message);
        }
        catch (IOException ex) {
            logger.LogError(ex, "无法创建导出文件，请检查导出路径");
            return ServiceResult.Error(ex.Message);
        }

        using var fileStream = stream;
        var topics = await dbContext.Topics
            .AsNoTracking()
            .Include(v => v.TopicOptions)
            .Where(v => topicIds.Contains(v.TopicId))
            .ToArrayAsync();
        try {
            ExcelTopicTranslator.TranslateTopicsToExcel(fileStream, topics);
            return ServiceResult.Ok();
        }
        catch (Exception ex) {
            logger.LogError(ex, "导出题目时出错");
            return ServiceResult.Error(ex.Message);
        }
    }

    public async Task<ServiceResult> ExportExcelFromTopicIdsAsync(Stream stream, int[] topicIds) {
        var topics = await dbContext.Topics
            .AsNoTracking()
            .Include(v => v.TopicOptions)
            .Where(v => topicIds.Contains(v.TopicId))
            .ToArrayAsync();
        try {
            ExcelTopicTranslator.TranslateTopicsToExcel(stream, topics);
            return ServiceResult.Ok();
        }
        catch (Exception ex) {
            logger.LogError(ex, "导出题目时出错");
            return ServiceResult.Error(ex.Message);
        }
    }
}
