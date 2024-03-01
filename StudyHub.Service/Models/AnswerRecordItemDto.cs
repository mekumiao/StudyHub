using StudyHub.Service.Base;
using StudyHub.Storage.Entities;

namespace StudyHub.Service.Models;

public class AnswerRecordItemFilter : IQueryableFilter<AnswerRecordItem> {
    public int? TopicSubjectId { get; set; }
    public DifficultyLevel? DifficultyLevel { get; set; }
    public AnswerRecordItemStatus? State { get; set; }
    public TopicType? TopicType { get; set; }
    /// <summary>
    /// 获取错题记录。包含 考核测评、模拟考试、我要练习 的未作答和做错的记录，
    /// </summary>
    public bool? IsIncorrectly { get; set; }

    public IQueryable<AnswerRecordItem> Build(IQueryable<AnswerRecordItem> queryable) {
        if (TopicSubjectId is not null and > 0) {
            queryable = queryable.Where(v => v.TopicSubjectId == TopicSubjectId.Value);
        }
        if (DifficultyLevel is not null and not Storage.Entities.DifficultyLevel.None) {
            queryable = queryable.Where(v => v.DifficultyLevel == DifficultyLevel.Value);
        }
        if (State is not null and not AnswerRecordItemStatus.None) {
            queryable = queryable.Where(v => v.State == State.Value);
        }
        if (TopicType is not null and not Storage.Entities.TopicType.None) {
            queryable = queryable.Where(v => v.TopicType == TopicType);
        }
        if (IsIncorrectly is true) {
            queryable = queryable
                .Where(v => v.IsSubmission == true)
                .Where(v => v.AnswerRecordType == AnswerRecordType.Evaluation || v.AnswerRecordType == AnswerRecordType.Simulation || v.AnswerRecordType == AnswerRecordType.Practice)
                .Where(v => v.State == AnswerRecordItemStatus.NoReply || v.State == AnswerRecordItemStatus.Incorrectly);
        }
        return queryable;
    }
}

public class AnswerRecordItemDto {
    public int AnswerRecordItemId { get; set; }
    public int Order { get; set; }
    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
    public TopicType TopicType { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public string TopicText { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public int AnswerRecordId { get; set; }
    public AnswerRecord AnswerRecord { get; set; } = null!;
    /// <summary>
    /// 答案:
    /// 1.单选题和多选题，多选题直接将答案拼接即可。如：ABC
    /// 2.判断题，0表示错，1表示对
    /// 3.填空题，直接填入文本
    /// 4.null表示未作答
    /// </summary>
    public string? AnswerText { get; set; }
    public AnswerRecordItemStatus Status { get; set; }
    public IEnumerable<TopicOptionDto> TopicOptions { get; set; } = [];
}
