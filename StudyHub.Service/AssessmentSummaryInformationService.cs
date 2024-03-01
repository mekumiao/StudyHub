using System.Data.Common;

using Microsoft.EntityFrameworkCore;

using StudyHub.Service.Base;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;

namespace StudyHub.Service;

public class AssessmentSummaryInformationDto {
    /// <summary>
    /// 考核题目总数
    /// </summary>
    public int EvaluationTopicCount { get; set; }
    /// <summary>
    /// 模拟题目总数
    /// </summary>
    public int SimulationTopicCount { get; set; }
    /// <summary>
    /// 练习题目总数
    /// </summary>
    public int PracticeTopicCount { get; set; }
    /// <summary>
    /// 错题总数
    /// </summary>
    public int IncorrectlyTopicCount { get; set; }
    /// <summary>
    /// 考试记录总数
    /// </summary>
    public int ExaminationRecordCount { get; set; }
    /// <summary>
    /// 练习记录总数
    /// </summary>
    public int PracticeAnswerRecordCount { get; set; }
    /// <summary>
    /// 错题重做记录总数
    /// </summary>
    public int RedoAnswerRecordCount { get; set; }
    /// <summary>
    /// 需要重做的题目总数
    /// </summary>
    public int NeedRedoTopicCount { get; set; }
}

public class AssessmentSummaryInformationService(StudyHubDbContext dbContext, ILogger<AssessmentSummaryInformationService> logger) {

    private IQueryable<Topic> TopicsWhere(int subjectId, DifficultyLevel level) {
        return dbContext.Topics
            .AsNoTracking()
            .Where(v => v.TopicSubjectId == subjectId)
            .Where(v => v.DifficultyLevel == level);
    }

    private IQueryable<AnswerRecord> AnswerRecordsWhere(int subjectId, DifficultyLevel level) {
        return dbContext.AnswerRecords
            .AsNoTracking()
            .Where(v => v.IsSubmission == true)
            .Where(v => v.TopicSubjectId == subjectId)
            .Where(v => v.DifficultyLevel == level);
    }

    private IQueryable<AnswerRecordItem> AnswerRecordItemsWhere(int subjectId, DifficultyLevel level) {
        return dbContext.AnswerRecordItems
            .AsNoTracking()
            .Where(v => v.IsSubmission == true)
            .Where(v => v.TopicSubjectId == subjectId)
            .Where(v => v.DifficultyLevel == level);
    }

    public async Task<ServiceResult<AssessmentSummaryInformationDto>> GetSummaryInformationAsync(int subjectId, DifficultyLevel level) {
        try {
            var summaryinfo = new AssessmentSummaryInformationDto {
                EvaluationTopicCount = await TopicsWhere(subjectId, level).Where(v => v.TopicBankFlag == TopicBankFlag.Evaluation).CountAsync(),
                SimulationTopicCount = await TopicsWhere(subjectId, level).Where(v => v.TopicBankFlag == TopicBankFlag.Simulation).CountAsync(),
                PracticeAnswerRecordCount = await AnswerRecordsWhere(subjectId, level).Where(v => v.AnswerRecordType == AnswerRecordType.Practice).CountAsync(),
                ExaminationRecordCount = await AnswerRecordsWhere(subjectId, level).Where(v => v.AnswerRecordType == AnswerRecordType.Evaluation || v.AnswerRecordType == AnswerRecordType.Simulation).CountAsync(),
                RedoAnswerRecordCount = await AnswerRecordsWhere(subjectId, level).Where(v => v.AnswerRecordType == AnswerRecordType.Redo).CountAsync(),
                IncorrectlyTopicCount = await AnswerRecordItemsWhere(subjectId, level)
                .Where(v => v.AnswerRecordType == AnswerRecordType.Evaluation
                            || v.AnswerRecordType == AnswerRecordType.Simulation
                            || v.AnswerRecordType == AnswerRecordType.Practice)
                .Where(v => v.State == AnswerRecordItemStatus.Incorrectly || v.State == AnswerRecordItemStatus.NoReply)
                .CountAsync(),
                NeedRedoTopicCount = await AnswerRecordItemsWhere(subjectId, level)
                .Where(v => v.AnswerRecordType == AnswerRecordType.Evaluation
                            || v.AnswerRecordType == AnswerRecordType.Simulation
                            || v.AnswerRecordType == AnswerRecordType.Practice)
                .Where(v => v.State == AnswerRecordItemStatus.Incorrectly || v.State == AnswerRecordItemStatus.NoReply)
                .Where(v => v.IsRedoCorrectly == false).CountAsync()
            };
            summaryinfo.PracticeTopicCount = summaryinfo.SimulationTopicCount;
            return ServiceResult.Ok(summaryinfo);
        }
        catch (DbException ex) {
            logger.LogError(ex, "汇总考核数据时发生错误");
            return ServiceResult.Error<AssessmentSummaryInformationDto>(ex.Message);
        }
    }
}
