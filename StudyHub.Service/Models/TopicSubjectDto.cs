using StudyHub.Service.Base;
using StudyHub.Storage.Entities;

namespace StudyHub.Service.Models;

public class TopicSubjectFilter : IQueryableFilter<TopicSubject> {
    public string? Name { get; set; }

    public IQueryable<TopicSubject> Build(IQueryable<TopicSubject> queryable) {
        if (string.IsNullOrWhiteSpace(Name) is false) {
            queryable.Where(v => v.Name.Contains(Name));
        }
        return queryable;
    }
}

public class TopicSubjectDto {
    public bool IsChecked { get; set; }
    public int TopicSubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TopicSubjectCreate {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TopicSubjectUpdate {
    public string? Name { get; set; }
    public string? Description { get; set; }
}
