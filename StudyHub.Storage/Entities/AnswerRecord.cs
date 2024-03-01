using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StudyHub.Storage.Entities;

/// <summary>
/// 答题记录类型
/// </summary>
public enum AnswerRecordType {
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None,
    /// <summary>
    /// 测评
    /// </summary>
    [Description("考核测评")]
    Evaluation,
    /// <summary>
    /// 模拟
    /// </summary>
    [Description("模拟考试")]
    Simulation,
    /// <summary>
    /// 练习
    /// </summary>
    [Description("我要练习")]
    Practice,
    /// <summary>
    /// 错题重做
    /// </summary>
    [Description("错题重做")]
    Redo,
}

/// <summary>
/// 答题记录
/// </summary>
public class AnswerRecord {
    [Key]
    public int AnswerRecordId { get; set; }
    /// <summary>
    /// 答题记录类型
    /// </summary>
    public AnswerRecordType AnswerRecordType { get; set; }
    /// <summary>
    /// 科目ID（不做外键，仅方便查询）
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
    public List<AnswerRecordItem> AnswerRecordItems { get; } = [];
}
