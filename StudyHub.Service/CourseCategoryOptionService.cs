using Microsoft.EntityFrameworkCore;

using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;

namespace StudyHub.Service;

public class CourseCategoryOptionService(StudyHubDbContext dbContext) {
    public async Task<LabelValueOption[]> GetOptionsAsync() {
        return await dbContext.CourseCategories.AsNoTracking().OrderBy(v => v.Order).ThenByDescending(v => v.CourseCategoryId).Select(v => new LabelValueOption {
            Id = v.CourseCategoryId,
            Text = v.Name,
        }).ToArrayAsync();
    }

    public async Task<LabelValueOption[]> GetOptionsWithDefaultAsync() {
        return [LabelValueOption.Default, .. await GetOptionsAsync()];
    }
}
