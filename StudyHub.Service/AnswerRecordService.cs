using System.Data.Common;

using Mapster;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;

namespace StudyHub.Service;

internal class AnswerRecordServiceRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<AnswerRecord, AnswerRecordDto>()
            .AfterMapping((src, dest) => dest.AnswerRecordItems = dest.AnswerRecordItems.OrderBy(v => v.Order));

        config.NewConfig<AnswerRecordItem, AnswerRecordItemDto>()
            .Map(dest => dest.Status, src => src.State)
            .Map(dest => dest.TopicOptions, src => src.Topic.TopicOptions, should => should.Topic != null);

        config.NewConfig<TopicOption, TopicOptionDto>();

        config.ForDestinationType<AnswerRecord>()
            .Map(dest => dest.StartTime, () => DateTime.UtcNow);

        config.NewConfig<EvaluationRecordCreate, AnswerRecordCreate>()
            .Map(dest => dest.AnswerRecordType, src => AnswerRecordType.Evaluation);

        config.NewConfig<SimulationRecordCreate, AnswerRecordCreate>()
            .Map(dest => dest.AnswerRecordType, src => AnswerRecordType.Simulation);

        config.NewConfig<PracticeRecordCreate, AnswerRecordCreate>()
            .Map(dest => dest.AnswerRecordType, src => AnswerRecordType.Practice);

        config.NewConfig<RedoingRecordCreate, AnswerRecordCreate>()
            .Map(dest => dest.AnswerRecordType, src => AnswerRecordType.Redo);
    }
}

/// <summary>
/// 答题记录（相当于试卷）
/// </summary>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
public class AnswerRecordService(
    StudyHubDbContext dbContext,
    ILogger<AnswerRecordService> logger,
    IMapper mapper,
    IAnswerVerificationService answerVerificationService) {
    private const string NotFountRecordItemMessage = "没有找到可用的题目";

    public async Task<ServiceResult<AnswerRecordDto>> GetEntityByIdAsync(int id) {
        var item = await dbContext.AnswerRecords.AsNoTracking()
            .Include(v => v.AnswerRecordItems)
            .ThenInclude(v => v.Topic)
            .ThenInclude(v => v.TopicOptions)
            .SingleOrDefaultAsync(v => v.AnswerRecordId == id);
        if (item is null) {
            return ServiceResult.NotFound<AnswerRecordDto>();
        }
        var result = mapper.Map<AnswerRecordDto>(item);
        return ServiceResult.Ok(result);
    }

    public async Task<ServiceResult<PagingResult<AnswerRecordDto>>> GetListAsync(AnswerRecordFilter filter, Paging paging) {
        var queryable = dbContext.AnswerRecords.AsNoTracking();

        queryable = filter.Build(queryable);
        var total = await queryable.CountAsync();

        queryable = paging.Build(queryable.OrderByDescending(v => v.AnswerRecordId));
        var result = await queryable.ToArrayAsync();

        var resultItems = mapper.Map<AnswerRecordDto[]>(result);
        var pageResult = new PagingResult<AnswerRecordDto>(paging, total, resultItems);
        return ServiceResult.Ok(pageResult);
    }

    /// <summary>
    /// 获取记录明细
    /// </summary>
    /// <returns></returns>
    public async Task<ServiceResult<AnswerRecordItemDto[]>> GetAnswerRecordItemAsync(AnswerRecordItemFilter filter, Paging paging) {
        var queryable = dbContext.AnswerRecordItems
            .AsNoTracking()
            .OrderBy(v => v.TopicType)
            .Include(v => v.Topic)
            .ThenInclude(v => v.TopicOptions)
            .AsQueryable();
        queryable = filter.Build(queryable);
        var total = await queryable.CountAsync();

        queryable = paging.Build(queryable);
        var result = await queryable.ToArrayAsync();
        LoopAnswerRecordItemToSetValueOfOrderProperty(result);
        var resultItems = mapper.Map<AnswerRecordItemDto[]>(result);
        return ServiceResult.Ok(resultItems);
    }

    public async Task<ServiceResult> ResetValueOfIsRedoCorrectlyField(int subjectId, DifficultyLevel level) {
        await dbContext.AnswerRecordItems
             .Where(v => v.TopicSubjectId == subjectId)
             .Where(v => v.DifficultyLevel == level)
             .ExecuteUpdateAsync(p => p.SetProperty(v => v.IsRedoCorrectly, false));
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> CleanAsync(AnswerRecordFilter filter) {
        var queryable = dbContext.AnswerRecords.AsQueryable();
        queryable = filter.Build(queryable);
        await queryable.ExecuteDeleteAsync();
        return ServiceResult.Ok();
    }

    private IQueryable<T> RandomQueryable<T>(IQueryable<T> queryable) where T : class {
        queryable = dbContext.Database.ProviderName?.Contains("sqlserver", StringComparison.CurrentCultureIgnoreCase) is true
            ? queryable.OrderBy(x => Guid.NewGuid())
            : queryable.OrderBy(x => EF.Functions.Random());
        return queryable;
    }

    /// <summary>
    /// 循环设置记录项的题目序号。从1开始
    /// </summary>
    /// <param name="items"></param>
    private static void LoopAnswerRecordItemToSetValueOfOrderProperty(IEnumerable<AnswerRecordItem> items) {
        int i = 1;
        foreach (var item in items.OrderBy(v => v.TopicType)) {
            item.Order = i++;
        }
    }

    /// <summary>
    /// 根据Topics创建完整的AnswerRecord
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="answerRecord">AnswerRecord基本信息</param>
    /// <returns></returns>
    private async Task<ServiceResult<AnswerRecord>> CreateFullAnswerRecordFromTopics(IQueryable<Topic> queryable, AnswerRecord answerRecord) {
        queryable = queryable
            .AsNoTracking()
            .Where(v => v.DifficultyLevel == answerRecord.DifficultyLevel)
            .Where(v => v.TopicSubjectId == answerRecord.TopicSubjectId);
        queryable = RandomQueryable(queryable);

        var singleTopics = await queryable.Where(v => v.TopicType == TopicType.Single).Take(answerRecord.SingleCount).ToArrayAsync();
        var multipleTopics = await queryable.Where(v => v.TopicType == TopicType.Multiple).Take(answerRecord.MultipleCount).ToArrayAsync();
        var truefalseTopics = await queryable.Where(v => v.TopicType == TopicType.TrueFalse).Take(answerRecord.TrueFalseCount).ToArrayAsync();
        var fillTopics = await queryable.Where(v => v.TopicType == TopicType.Fill).Take(answerRecord.FillCount).ToArrayAsync();

        answerRecord.SingleCount = singleTopics.Length;
        answerRecord.MultipleCount = multipleTopics.Length;
        answerRecord.TrueFalseCount = truefalseTopics.Length;
        answerRecord.FillCount = fillTopics.Length;

        var topics = singleTopics.Concat(multipleTopics).Concat(truefalseTopics).Concat(fillTopics);
        var recordItems = mapper.Map<AnswerRecordItem[]>(topics);
        if (recordItems.Length == 0) {
            return ServiceResult.NotFound<AnswerRecord>(NotFountRecordItemMessage);
        }

        SynchronizeItemsInfoToAnswerRecord(answerRecord, recordItems);

        await dbContext.AnswerRecords.AddAsync(answerRecord);
        await dbContext.SaveChangesAsync();

        return ServiceResult.Ok(answerRecord);
    }

    /// <summary>
    /// 同步AnswerRecordItem信息到AnswerRecord
    /// </summary>
    /// <remarks>
    /// 计算总分、设置Order、统计题目总数、将<see cref="AnswerRecordItem"/>附加到<see cref="AnswerRecord.AnswerRecordItems"/>中
    /// </remarks>
    /// <param name="answerRecord"></param>
    /// <param name="recordItems"></param>
    private static void SynchronizeItemsInfoToAnswerRecord(AnswerRecord answerRecord, AnswerRecordItem[] recordItems) {
        // 计算总分
        answerRecord.TotalScore = recordItems.Sum(v => GetScoreOfAnswerRecordItem(answerRecord, v, AnswerRecordItemStatus.None));
        // 设置Order
        LoopAnswerRecordItemToSetValueOfOrderProperty(recordItems);

        foreach (var item in recordItems) {
            // 设置每一项的答题类型，方便在错题重做中使用
            item.AnswerRecordType = answerRecord.AnswerRecordType;
        }

        answerRecord.TotalTopic = recordItems.Length;
        answerRecord.AnswerRecordItems.AddRange(recordItems);
    }

    /// <summary>
    /// 创建考核测评
    /// </summary>
    /// <param name="create"></param>
    /// <returns></returns>
    public async Task<ServiceResult<AnswerRecordDto>> CreateAsync(EvaluationRecordCreate create) {
        var answerRecord = mapper.Map<AnswerRecord>(mapper.Map<AnswerRecordCreate>(create));
        mapper.Map(create, answerRecord);

        var queryable = dbContext.Topics.Where(v => v.TopicBankFlag == TopicBankFlag.Evaluation);
        var result = await CreateFullAnswerRecordFromTopics(queryable, answerRecord);
        if (result.IsSuccess is false) {
            return ServiceResult.Error<AnswerRecordDto>(result.Message);
        }

        return await GetEntityByIdAsync(result.Result.AnswerRecordId);
    }

    /// <summary>
    /// 创建模拟考试
    /// </summary>
    /// <param name="create"></param>
    /// <returns></returns>
    public async Task<ServiceResult<AnswerRecordDto>> CreateAsync(SimulationRecordCreate create) {
        var answerRecord = mapper.Map<AnswerRecord>(mapper.Map<AnswerRecordCreate>(create));
        mapper.Map(create, answerRecord);

        var queryable = dbContext.Topics.Where(v => v.TopicBankFlag == TopicBankFlag.Simulation);
        var result = await CreateFullAnswerRecordFromTopics(queryable, answerRecord);
        if (result.IsSuccess is false) {
            return ServiceResult.Error<AnswerRecordDto>(result.Message);
        }

        return await GetEntityByIdAsync(result.Result.AnswerRecordId);
    }

    /// <summary>
    /// 创建我要练习
    /// </summary>
    /// <param name="create"></param>
    /// <returns></returns>
    public async Task<ServiceResult<AnswerRecordDto>> CreateAsync(PracticeRecordCreate create) {
        var answerRecord = mapper.Map<AnswerRecord>(mapper.Map<AnswerRecordCreate>(create));
        mapper.Map(create, answerRecord);

        var queryable = dbContext.Topics.Where(v => v.TopicBankFlag == TopicBankFlag.Simulation);
        var result = await CreateFullAnswerRecordFromTopics(queryable, answerRecord);
        if (result.IsSuccess is false) {
            return ServiceResult.Error<AnswerRecordDto>(result.Message);
        }

        return await GetEntityByIdAsync(result.Result.AnswerRecordId);
    }

    /// <summary>
    /// 创建错题重做记录
    /// </summary>
    /// <param name="create"></param>
    /// <returns></returns>
    public async Task<ServiceResult<AnswerRecordDto>> CreateAsync(RedoingRecordCreate create) {
        var answerRecord = mapper.Map<AnswerRecord>(mapper.Map<AnswerRecordCreate>(create));
        mapper.Map(create, answerRecord);

        var queryable = dbContext.AnswerRecordItems
            .AsNoTracking()
            .Where(v => v.IsSubmission == true)
            .Where(v => v.TopicSubjectId == create.TopicSubjectId)
            .Where(v => v.DifficultyLevel == create.DifficultyLevel)
            .Where(v => v.State == AnswerRecordItemStatus.Incorrectly || v.State == AnswerRecordItemStatus.NoReply)
            .Where(v => v.AnswerRecordType == AnswerRecordType.Evaluation
            || v.AnswerRecordType == AnswerRecordType.Simulation
            || v.AnswerRecordType == AnswerRecordType.Practice)
            .Where(v => v.IsRedoCorrectly == false);
        queryable = RandomQueryable(queryable);

        var singleTopics = await queryable.Where(v => v.TopicType == TopicType.Single).Take(create.SingleCount).ToArrayAsync();
        var multipleTopics = await queryable.Where(v => v.TopicType == TopicType.Multiple).Take(create.MultipleCount).ToArrayAsync();
        var truefalseTopics = await queryable.Where(v => v.TopicType == TopicType.TrueFalse).Take(create.TrueFalseCount).ToArrayAsync();
        var fillTopics = await queryable.Where(v => v.TopicType == TopicType.Fill).Take(create.FillCount).ToArrayAsync();

        answerRecord.SingleCount = singleTopics.Length;
        answerRecord.MultipleCount = multipleTopics.Length;
        answerRecord.TrueFalseCount = truefalseTopics.Length;
        answerRecord.FillCount = fillTopics.Length;

        var recordItems = singleTopics.Concat(multipleTopics).Concat(truefalseTopics).Concat(fillTopics).ToArray();
        if (recordItems.Length == 0) {
            return ServiceResult.NotFound<AnswerRecordDto>(NotFountRecordItemMessage);
        }

        foreach (var item in recordItems) {
            item.AnswerRecordType = answerRecord.AnswerRecordType;
            item.IsSubmission = false;
            // 重要！记录错题来源
            item.RedoFromAnswerRecordItemId = item.AnswerRecordItemId;
            item.AnswerRecordId = default;
            item.AnswerRecord = default!;
            item.AnswerRecordItemId = default;
            item.AnswerText = default;
            item.State = AnswerRecordItemStatus.None;
        }

        LoopAnswerRecordItemToSetValueOfOrderProperty(recordItems);

        answerRecord.TotalTopic = recordItems.Length;
        answerRecord.TotalScore = recordItems.Sum(v => GetScoreOfAnswerRecordItem(answerRecord, v, AnswerRecordItemStatus.None));
        answerRecord.AnswerRecordItems.AddRange(recordItems);

        await dbContext.AnswerRecords.AddAsync(answerRecord);
        await dbContext.SaveChangesAsync();

        return await GetEntityByIdAsync(answerRecord.AnswerRecordId);
    }

    private static TimeSpan CalculateTimeInterval(DateTime? start, DateTime? end) {
        if (start.HasValue is false) {
            return TimeSpan.Zero;
        }
        if (end.HasValue is false) {
            return TimeSpan.Zero;
        }
        return (end - start).Value;
    }

    private static bool IsSubmissionTimeout(int total, int current) {
        if (total <= 0) {
            return false;
        }
        if (current <= 0) {
            return false;
        }
        return current > total;
    }

    /// <summary>
    /// 交卷
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<ServiceResult> SubmissionAsync(SubmissionModel model) {
        var answerRecord = await dbContext.AnswerRecords
            .Include(v => v.AnswerRecordItems)
            .SingleOrDefaultAsync(v => v.AnswerRecordId == model.AnswerRecordId);
        if (answerRecord is null) {
            return ServiceResult.NotFound();
        }

        answerRecord.SubmissionTime = DateTime.UtcNow;
        answerRecord.TimeTakenSeconds = (int)CalculateTimeInterval(answerRecord.StartTime, answerRecord.SubmissionTime).TotalSeconds;
        answerRecord.IsSubmission = true;
        answerRecord.IsTimeout = IsSubmissionTimeout(answerRecord.DurationSeconds, answerRecord.TimeTakenSeconds);

        var items = from i in answerRecord.AnswerRecordItems
                    join m in model.SubmissionItems on i.AnswerRecordItemId equals m.AnswerRecordItemId
                    select new { recordItem = i, submissionItem = m };
        foreach (var item in items) {
            item.recordItem.IsSubmission = true;
            item.recordItem.AnswerText = item.submissionItem.AnswerText?.Trim();
            item.recordItem.State = string.IsNullOrWhiteSpace(item.recordItem.AnswerText) ? AnswerRecordItemStatus.NoReply : AnswerRecordItemStatus.Answer;
            if (item.recordItem.State is AnswerRecordItemStatus.Answer) {
                answerRecord.TotalAnswer += 1;
            }
        }

        await dbContext.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public static int GetTopicCountOfAnswerRecordItem(AnswerRecord answerRecord, AnswerRecordItem item) {
        return item.TopicType switch {
            TopicType.Single => answerRecord.SingleCount,
            TopicType.Multiple => answerRecord.MultipleCount,
            TopicType.TrueFalse => answerRecord.TrueFalseCount,
            TopicType.Fill => answerRecord.FillCount,
            _ => 0,
        };
    }

    public static int GetScoreOfAnswerRecordItem(AnswerRecord answerRecord, AnswerRecordItem item, AnswerRecordItemStatus status) {
        return item.TopicType switch {
            TopicType.Single => item.State == status ? answerRecord.SingleScore : 0,
            TopicType.Multiple => item.State == status ? answerRecord.MultipleScore : 0,
            TopicType.TrueFalse => item.State == status ? answerRecord.TrueFalseScore : 0,
            TopicType.Fill => item.State == status ? answerRecord.FillScore : 0,
            _ => 0,
        };
    }

    /// <summary>
    /// 批改作答
    /// </summary>
    /// <param name="answerRecordId"></param>
    /// <returns></returns>
    public async Task<ServiceResult> CorrectingAnswerRecordAsync(int answerRecordId) {
        var answerRecord = await dbContext.AnswerRecords
            .Include(v => v.AnswerRecordItems)
            .SingleOrDefaultAsync(v => v.AnswerRecordId == answerRecordId);
        if (answerRecord is null) {
            return ServiceResult.NotFound();
        }

        answerRecord.Score = 0;

        foreach (var item in answerRecord.AnswerRecordItems) {
            if (string.IsNullOrWhiteSpace(item.AnswerText)) {
                continue;
            }
            item.State = answerVerificationService.Verification(item.AnswerText, item.CorrectAnswer, item.TopicType) ? AnswerRecordItemStatus.Correct : AnswerRecordItemStatus.Incorrectly;
            answerRecord.Score += GetScoreOfAnswerRecordItem(answerRecord, item, AnswerRecordItemStatus.Correct);
            if (item.State is AnswerRecordItemStatus.Correct) {
                answerRecord.TotalCorrect += 1;
            }
            else if (item.State is AnswerRecordItemStatus.Incorrectly) {
                answerRecord.TotalIncorrect += 1;
            }
        }

        // 如果是错题重做，就设置关联的源错题IsRedoCorrectly字段值。防止错题被重复做
        if (answerRecord.AnswerRecordType is AnswerRecordType.Redo) {
            var ids = answerRecord.AnswerRecordItems
                .Where(v => v.State == AnswerRecordItemStatus.Correct)
                .Where(v => v.RedoFromAnswerRecordItemId.HasValue)
                .Select(v => v.RedoFromAnswerRecordItemId!.Value).ToArray();
            var items = await dbContext.AnswerRecordItems.Where(v => ids.Contains(v.AnswerRecordItemId)).ToArrayAsync();
            foreach (var o in items) {
                o.IsRedoCorrectly = true;
            }
        }

        await dbContext.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    /// <summary>
    /// 记录用户的作答
    /// </summary>
    /// <param name="submission"></param>
    /// <returns></returns>
    public async Task<ServiceResult> RecordUserAnswerAsync(SubmissionItemModel submission) {
        var item = await dbContext.AnswerRecordItems.FindAsync(submission.AnswerRecordItemId);
        if (item is null) {
            return ServiceResult.NotFound();
        }

        item.AnswerText = submission.AnswerText?.Trim();
        item.State = string.IsNullOrWhiteSpace(item.AnswerText) ? AnswerRecordItemStatus.NoReply : AnswerRecordItemStatus.Answer;

        try {
            await dbContext.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (DbException ex) {
            logger.LogError(ex, "记录用户作答时失败 {answerRecordItemId} {userAnswerText}", submission.AnswerRecordItemId, submission.AnswerText);
            return ServiceResult.Error(ex.Message);
        }
    }
}
