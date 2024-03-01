using System.ComponentModel.DataAnnotations;

namespace StudyHub.Storage.Entities;

public class Course {
    [Key]
    public int CourseId { get; set; }
    public string Cover { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// 课节数
    /// </summary>
    public int SectionCount { get; set; }
    public int CourseCategoryId { get; set; }
    public CourseCategory CourseCategory { get; set; } = null!;
    public List<CourseSection> CourseSections { get; } = [];
}
