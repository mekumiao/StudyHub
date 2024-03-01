using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using StudyHub.Storage.DbContexts;

namespace StudyHubDb;

public class SeedData {
    public static async Task EnsureSeedDataAsync(IServiceProvider serviceProvider) {
        using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
        using var context = scope.ServiceProvider.GetRequiredService<StudyHubDbContext>();
        await context.Database.MigrateAsync();
    }
}
