using StudyHub.Service.Base;
using StudyHub.Storage.Entities;

namespace StudyHub.Service.Models;

public class CourseCategoryFilter : IQueryableFilter<CourseCategory> {
    public string? Name { get; set; }

    public IQueryable<CourseCategory> Build(IQueryable<CourseCategory> queryable) {
        if (string.IsNullOrWhiteSpace(Name) is false) {
            queryable = queryable.Where(v => v.Name.Contains(Name));
        }
        return queryable;
    }
}

public class CourseCategoryDto {
    public int CourseCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
}

public class CourseCategoryCreate {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CourseCategoryUpdate {
    public string? Name { get; set; }
    public string? Description { get; set; }
}
