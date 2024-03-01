using Microsoft.EntityFrameworkCore;

using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;

namespace StudyHub.Service;

public class TopicSubjectOptionService(StudyHubDbContext dbContext) {
    public async Task<LabelValueOption[]> GetOptionsAsync() {
        return await dbContext.TopicSubjects.AsNoTracking().OrderBy(v => v.TopicSubjectId).Select(v => new LabelValueOption {
            Id = v.TopicSubjectId,
            Text = v.Name,
        }).ToArrayAsync();
    }

    public async Task<LabelValueOption[]> GetOptionsWithDefaultAsync() {
        return [LabelValueOption.Default, .. await GetOptionsAsync()];
    }
}
