using System.ComponentModel.DataAnnotations;

namespace StudyHub.Storage.Entities;

/// <summary>
/// 科目
/// </summary>
public class TopicSubject {
    [Key]
    public int TopicSubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Topic> Topics { get; } = [];
}
