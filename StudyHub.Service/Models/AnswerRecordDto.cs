using StudyHub.Service.Base;
using StudyHub.Storage.Entities;

namespace StudyHub.Service.Models;

public class AnswerRecordFilter : IQueryableFilter<AnswerRecord> {
    public AnswerRecordType? AnswerRecordType { get; set; }
    public DifficultyLevel? DifficultyLevel { get; set; }
    public bool? IsSubmission { get; set; }
    /// <summary>
    /// 是否仅获取考试记录
    /// </summary>
    public bool? IsExamination { get; set; }
    public int? TopicSubjectId { get; set; }
    /// <summary>
    /// 获取可以继续作答的记录。未交卷、开始时间+考试限制时间<当前时间
    /// </summary>
    public bool? ShouldContinueAnswering { get; set; }

    public IQueryable<AnswerRecord> Build(IQueryable<AnswerRecord> queryable) {
        if (AnswerRecordType is not null and > Storage.Entities.AnswerRecordType.None) {
            queryable = queryable.Where(v => v.AnswerRecordType == AnswerRecordType.Value);
        }
        if (DifficultyLevel is not null and > Storage.Entities.DifficultyLevel.None) {
            queryable = queryable.Where(v => v.DifficultyLevel == DifficultyLevel.Value);
        }
        if (IsSubmission is not null) {
            queryable = queryable.Where(v => v.IsSubmission == IsSubmission.Value);
        }
        if (IsExamination is not null) {
            queryable = queryable.Where(v => v.AnswerRecordType == Storage.Entities.AnswerRecordType.Evaluation || v.AnswerRecordType == Storage.Entities.AnswerRecordType.Simulation);
        }
        if (TopicSubjectId is not null) {
            queryable = queryable.Where(v => v.TopicSubjectId == TopicSubjectId.Value);
        }
        if (ShouldContinueAnswering is not null) {
            queryable = queryable.Where(v => v.IsSubmission == false)
                .Where(v => v.StartTime > DateTime.UtcNow.AddSeconds(-v.DurationSeconds + 10));// 排除仅剩10秒答题时间的记录
        }
        return queryable;
    }
}

public class AnswerRecordDto {
    public int AnswerRecordId { get; set; }
    /// <summary>
    /// 答题记录类型
    /// </summary>
    public AnswerRecordType AnswerRecordType { get; set; }
    /// <summary>
    /// 科目ID
    /// </summary>
    public int TopicSubjectId { get; set; }
    /// <summary>
    /// 难度等级
    /// </summary>
    public DifficultyLevel DifficultyLevel { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 交卷时间
    /// </summary>
    public DateTime? SubmissionTime { get; set; }
    /// <summary>
    /// 限制的时间（秒）
    /// </summary>
    public int DurationSeconds { get; set; }
    /// <summary>
    /// 答题所用的时间（秒）
    /// </summary>
    public int TimeTakenSeconds { get; set; }
    /// <summary>
    /// 是否交卷
    /// </summary>
    public bool IsSubmission { get; set; }
    /// <summary>
    /// 是否超时
    /// </summary>
    public bool IsTimeout { get; set; }
    /// <summary>
    /// 题目总数
    /// </summary>
    public int TotalTopic { get; set; }
    /// <summary>
    /// 作答总数
    /// </summary>
    public int TotalAnswer { get; set; }
    /// <summary>
    /// 正确总数
    /// </summary>
    public int TotalCorrect { get; set; }
    /// <summary>
    /// 错题总数
    /// </summary>
    public int TotalIncorrect { get; set; }
    /// <summary>
    /// 单选题分数
    /// </summary>
    public int SingleScore { get; set; }
    /// <summary>
    /// 多选题分数
    /// </summary>
    public int MultipleScore { get; set; }
    /// <summary>
    /// 判断题分数
    /// </summary>
    public int TrueFalseScore { get; set; }
    /// <summary>
    /// 填空题分数
    /// </summary>
    public int FillScore { get; set; }
    /// <summary>
    /// 总分
    /// </summary>
    public int TotalScore { get; set; }
    /// <summary>
    /// 得分
    /// </summary>
    public int Score { get; set; }
    /// <summary>
    /// 单选题数量
    /// </summary>
    public int SingleCount { get; set; }
    /// <summary>
    /// 多选题数量
    /// </summary>
    public int MultipleCount { get; set; }
    /// <summary>
    /// 判断题数量
    /// </summary>
    public int TrueFalseCount { get; set; }
    /// <summary>
    /// 填空题数量
    /// </summary>
    public int FillCount { get; set; }
    /// <summary>
    /// 题目列表
    /// </summary>
    public IEnumerable<AnswerRecordItemDto> AnswerRecordItems { get; set; } = [];
}

public class AnswerRecordCreate {
    /// <summary>
    /// 答题记录类型
    /// </summary>
    public AnswerRecordType AnswerRecordType { get; set; }
    /// <summary>
    /// 科目ID
    /// </summary>
    public int TopicSubjectId { get; set; }
    /// <summary>
    /// 难度等级
    /// </summary>
    public DifficultyLevel DifficultyLevel { get; set; }
    /// <summary>
    /// 限制的时间（秒）
    /// </summary>
    public int DurationSeconds { get; set; }
    /// <summary>
    /// 单选题分数
    /// </summary>
    public int SingleScore { get; set; } = 1;
    /// <summary>
    /// 多选题分数
    /// </summary>
    public int MultipleScore { get; set; } = 1;
    /// <summary>
    /// 判断题分数
    /// </summary>
    public int TrueFalseScore { get; set; } = 1;
    /// <summary>
    /// 填空题分数
    /// </summary>
    public int FillScore { get; set; } = 1;
}

/// <summary>
/// 考核记录创建模型
/// </summary>
public record EvaluationRecordCreate(
    int TopicSubjectId,
    DifficultyLevel DifficultyLevel,
    int SingleCount,
    int SingleScore,
    int MultipleCount,
    int MultipleScore,
    int TrueFalseCount,
    int TrueFalseScore,
    int FillCount,
    int FillScore,
    int DurationSeconds);

/// <summary>
/// 模拟考试记录创建模型
/// </summary>
/// <param name="TopicSubjectId"></param>
/// <param name="DifficultyLevel"></param>
/// <param name="SingleCount"></param>
/// <param name="SingleScore"></param>
/// <param name="MultipleCount"></param>
/// <param name="MultipleScore"></param>
/// <param name="TrueFalseCount"></param>
/// <param name="TrueFalseScore"></param>
/// <param name="FillCount"></param>
/// <param name="FillScore"></param>
public record SimulationRecordCreate(
    int TopicSubjectId,
    DifficultyLevel DifficultyLevel,
    int SingleCount,
    int SingleScore,
    int MultipleCount,
    int MultipleScore,
    int TrueFalseCount,
    int TrueFalseScore,
    int FillCount,
    int FillScore,
    int DurationSeconds);

/// <summary>
/// 我要练习创建模型
/// </summary>
/// <param name="TopicSubjectId"></param>
/// <param name="DifficultyLevel"></param>
/// <param name="SingleCount"></param>
/// <param name="MultipleCount"></param>
/// <param name="TrueFalseCount"></param>
/// <param name="FillCount"></param>
public record PracticeRecordCreate(
    int TopicSubjectId,
    DifficultyLevel DifficultyLevel,
    int SingleCount,
    int MultipleCount,
    int TrueFalseCount,
    int FillCount);

/// <summary>
/// 重做记录创建模型
/// </summary>
/// <param name="TopicSubjectId"></param>
/// <param name="DifficultyLevel"></param>
/// <param name="SingleCount"></param>
/// <param name="MultipleCount"></param>
/// <param name="TrueFalseCount"></param>
/// <param name="FillCount"></param>
public record RedoingRecordCreate(
    int TopicSubjectId,
    DifficultyLevel DifficultyLevel,
    int SingleCount,
    int MultipleCount,
    int TrueFalseCount,
    int FillCount);
