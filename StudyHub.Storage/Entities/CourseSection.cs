using System.ComponentModel.DataAnnotations;

namespace StudyHub.Storage.Entities;

public class CourseSection {
    [Key]
    public int CourseSectionId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
