using System.ComponentModel.DataAnnotations;

namespace StudyHub.Storage.Entities;

public class Setting {
    [Key]
    public int SettingId { get; set; }
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string SettingType { get; set; } = string.Empty;
    public string SettingDescription { get; set; } = string.Empty;
}

public record SettingToken(string Type, string Name, string DefaultValue, string Description);

public sealed class SettingTypeConstants {
    /// <summary>
    /// 考核系统
    /// </summary>
    public static readonly string Exam = "Exam";
    public static readonly string System = "System";
}

public sealed class SettingNameConstants {
    /// <summary>
    /// 考核测评参数
    /// </summary>
    public static readonly string ExamParameter = "ExamParameter";
    public static readonly string IsSyncCourses = "IsSyncCourses";
    public static readonly string IsSyncTopics = "IsSyncTopics";
}

public static class SettingConstants {
    public static readonly SettingToken ExamParameter =
        new(SettingTypeConstants.Exam, SettingNameConstants.ExamParameter, "", "考核测评参数");
    /// <summary>
    /// 值有 0,1
    /// </summary>
    public static readonly SettingToken IsSyncCourses =
        new(SettingTypeConstants.System, SettingNameConstants.IsSyncCourses, BooleanConstant.False, "是否同步过课程");
    /// <summary>
    /// 值有 0,1
    /// </summary>
    public static readonly SettingToken IsSyncTopics =
        new(SettingTypeConstants.System, SettingNameConstants.IsSyncTopics, BooleanConstant.False, "是否同步过题目");
}

public static class BooleanConstant {
    public const string False = "0";
    public const string True = "1";
}
