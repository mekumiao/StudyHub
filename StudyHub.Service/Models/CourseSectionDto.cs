namespace StudyHub.Service.Models;

public class CourseSectionDto {
    public int CourseSectionId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int CourseId { get; set; }
}

public class CourseSectionCreate {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public int CourseCategoryId { get; set; }
    public int Order { get; set; }
}
