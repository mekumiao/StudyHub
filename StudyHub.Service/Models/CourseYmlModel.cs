using YamlDotNet.Serialization;

namespace StudyHub.Service.Models;

public class CourseYmlModel {
    [YamlIgnore]
    public string RelativePath { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Cover { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool Deleted { get; set; }
    public IEnumerable<CourseSectionYmlModel> Sections { get; set; } = [];
}

public class CourseSectionYmlModel {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    [YamlIgnore]
    public int Order { get; set; }
}
