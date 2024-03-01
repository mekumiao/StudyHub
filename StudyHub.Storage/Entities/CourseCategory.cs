using System.ComponentModel.DataAnnotations;

namespace StudyHub.Storage.Entities;

public class CourseCategory {
    [Key]
    public int CourseCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<Course> Courses { get; } = [];
}
