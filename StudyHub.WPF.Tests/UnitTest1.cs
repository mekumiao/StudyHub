using System.Collections.ObjectModel;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using StudyHub.Storage.DbContexts;
using StudyHub.WPF.Models;

using StudyHubDb;

namespace StudyHub.WPF.Tests;

[TestClass]
public class UnitTest1 {
    [TestMethod]
    public void TestMethod1() {
        var list = new ObservableCollection<string>(["x", "tt"]);
        list.RemoveAt(0);

        var list2 = new ObservableCollection<NotifyCheckedChanged<string>>([
            new NotifyCheckedChanged<string>("xxx"),
            new NotifyCheckedChanged<string>("111"),
        ]);

        list2.RemoveAt(0);

        var list3 = new ObservableCollection<NotifyCheckedChanged<string>>([
            new NotifyCheckedChanged<string>("xxx"),
            new NotifyCheckedChanged<string>("111"),
        ]);
        _ = new SupportsSelectAllOfDataContext<string> {
            Items = list3
        };
        list3.RemoveAt(0);
    }

    [TestMethod]
    public void TestMethod2() {
        File.Delete("Test.pddd");
    }

    [TestMethod]
    public async Task TestMethod3() {
        var services = new ServiceCollection();
        services.AddDbContext<StudyHubDbContext>(options => options.UseSqlite("Data Source=studyhub.db", dbOpts => {
            dbOpts.MigrationsAssembly(typeof(SeedData).Assembly.FullName);
            dbOpts.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));
        var serviceProvider = services.BuildServiceProvider();
        await SeedData.EnsureSeedDataAsync(serviceProvider);

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StudyHubDbContext>();
        {
            using var trans = dbContext.Database.BeginTransaction();
            trans.Commit();
        }
    }
}
