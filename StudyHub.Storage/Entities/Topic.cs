using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StudyHub.Storage.Entities;

/// <summary>
/// 题目表
/// </summary>
public class Topic {
    [Key]
    public int TopicId { get; set; }
    public TopicType TopicType { get; set; }
    public TopicBankFlag TopicBankFlag { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public string TopicText { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    /// <summary>
    /// 科目
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    public int TopicSubjectId { get; set; }
    public TopicSubject TopicSubject { get; set; } = null!;
    public List<TopicOption> TopicOptions { get; } = [];
    public List<AnswerRecordItem> AnswerRecordItems { get; } = [];
}

/// <summary>
/// 题型枚举
/// </summary>
public enum TopicType {
    [Description("无")]
    None,
    [Description("单选题")]
    Single,
    [Description("多选题")]
    Multiple,
    [Description("判断题")]
    TrueFalse,
    [Description("填空题")]
    Fill,
}

/// <summary>
/// 难度等级
/// </summary>
public enum DifficultyLevel {
    [Description("无")]
    None,
    [Description("初级")]
    Easy,
    [Description("中级")]
    Medium,
    [Description("高级")]
    Hard
}

public enum TopicBankFlag {
    [Description("无")]
    None,
    /// <summary>
    /// 考核
    /// </summary>
    [Description("考核")]
    Evaluation,
    /// <summary>
    /// 模拟
    /// </summary>
    [Description("模拟")]
    Simulation
}
