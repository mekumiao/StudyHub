using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using StudyHub.Storage.DbContexts;

using StudyHubDb;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddDbContext<StudyHubDbContext>(options =>
    options.UseSqlite("Data Source=studyhub.db", dbOpts =>
    dbOpts.MigrationsAssembly(typeof(SeedData).Assembly.FullName)));

var app = builder.Build();

app.Run();
