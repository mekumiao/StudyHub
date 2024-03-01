using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StudyHub.Storage.Entities;

public enum AnswerRecordItemStatus {
    [Description("无")]
    None,
    [Description("已答")]
    Answer,
    [Description("正确")]
    Correct,
    [Description("错误")]
    Incorrectly,
    [Description("未答")]
    NoReply,
}

public class AnswerRecordItem {
    [Key]
    public int AnswerRecordItemId { get; set; }
    /// <summary>
    /// 答题记录类型
    /// </summary>
    public AnswerRecordType AnswerRecordType { get; set; }
    /// <summary>
    /// 是否交卷
    /// </summary>
    public bool IsSubmission { get; set; }
    public int Order { get; set; }
    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
    /// <summary>
    /// 科目ID。不做外键，仅查询用
    /// </summary>
    public int TopicSubjectId { get; set; }
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
    /// <summary>
    /// 状态
    /// </summary>
    public AnswerRecordItemStatus State { get; set; }
    /// <summary>
    /// 是否被重做正确
    /// </summary>
    public bool IsRedoCorrectly { get; set; }
    /// <summary>
    /// 当前<see cref="AnswerRecordType"/>为<see cref="AnswerRecordType.Redo"/>时，标识来自的答题记录项ID。不做外键，仅做查询用
    /// </summary>
    public int? RedoFromAnswerRecordItemId { get; set; }
}
