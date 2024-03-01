using Microsoft.EntityFrameworkCore;

using StudyHub.Storage.Entities;

namespace StudyHub.Storage.DbContexts;

public class StudyHubDbContext(DbContextOptions options) : DbContext(options) {
    public DbSet<Topic> Topics { get; set; }
    public DbSet<TopicOption> TopicOptions { get; set; }
    public DbSet<TopicSubject> TopicSubjects { get; set; }
    public DbSet<AnswerRecord> AnswerRecords { get; set; }
    public DbSet<AnswerRecordItem> AnswerRecordItems { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseSection> CourseSections { get; set; }
    public DbSet<CourseCategory> CourseCategories { get; set; }
    public DbSet<Setting> Settings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);

        builder.Entity<Topic>()
            .HasMany(v => v.TopicOptions)
            .WithOne(v => v.Topic)
            .HasForeignKey(v => v.TopicId)
            .IsRequired();

        builder.Entity<TopicOption>()
            .Property(v => v.Code)
            .HasDefaultValue('A');

        builder.Entity<AnswerRecord>()
            .HasMany(v => v.AnswerRecordItems)
            .WithOne(v => v.AnswerRecord)
            .HasForeignKey(v => v.AnswerRecordId)
            .IsRequired();

        builder.Entity<AnswerRecordItem>()
            .HasOne(v => v.Topic)
            .WithMany(v => v.AnswerRecordItems)
            .HasForeignKey(v => v.TopicId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired();

        builder.Entity<TopicSubject>()
            .HasMany(v => v.Topics)
            .WithOne(v => v.TopicSubject)
            .HasForeignKey(v => v.TopicSubjectId)
            .IsRequired();

        builder.Entity<TopicSubject>()
            .HasIndex(v => v.Name)
            .IsUnique();

        builder.Entity<Course>()
            .HasMany(v => v.CourseSections)
            .WithOne(v => v.Course)
            .HasForeignKey(v => v.CourseId)
            .IsRequired();

        builder.Entity<CourseCategory>()
            .HasMany(v => v.Courses)
            .WithOne(v => v.CourseCategory)
            .HasForeignKey(v => v.CourseCategoryId)
            .IsRequired();

        builder.Entity<CourseCategory>()
            .HasIndex(v => v.Name)
            .IsUnique();

        builder.Entity<Setting>()
            .HasIndex(v => new { v.SettingName, v.SettingType })
            .IsUnique();
    }
}
