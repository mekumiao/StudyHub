namespace StudyHub.Storage.Entities;

/// <summary>
/// 选项表
/// </summary>
public class TopicOption {
    public int TopicOptionId { get; set; }
    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
    /// <summary>
    /// 选项编号。示例：A、B、C、D
    /// </summary>
    public char Code { get; set; }
    public string Text { get; set; } = string.Empty;
}
