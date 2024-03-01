using System.ComponentModel.DataAnnotations;

using StudyHub.Service.Base;
using StudyHub.Storage.Entities;

namespace StudyHub.Service.Models;

public class TopicFilter : IQueryableFilter<Topic> {
    public string? TopicTextOrId { get; set; }
    public TopicBankFlag? TopicBankFlag { get; set; }
    public TopicType? TopicType { get; set; }
    public DifficultyLevel? DifficultyLevel { get; set; }
    public int? TopicSubjectId { get; set; }

    public IQueryable<Topic> Build(IQueryable<Topic> queryable) {
        if (!string.IsNullOrWhiteSpace(TopicTextOrId)) {
            queryable = int.TryParse(TopicTextOrId, out var questionId)
                ? queryable.Where(v => v.TopicText.Contains(TopicTextOrId) || v.TopicId == questionId)
                : queryable.Where(v => v.TopicText.Contains(TopicTextOrId));
        }
        if (TopicBankFlag is not null and > 0) {
            queryable = queryable.Where(v => v.TopicBankFlag == TopicBankFlag.Value);
        }
        if (TopicType is not null and > 0) {
            queryable = queryable.Where(v => v.TopicType == TopicType);
        }
        if (DifficultyLevel is not null and > 0) {
            queryable = queryable.Where(v => v.DifficultyLevel == DifficultyLevel);
        }
        if (TopicSubjectId is not null and > 0) {
            queryable = queryable.Where(v => v.TopicSubjectId == TopicSubjectId);
        }
        return queryable;
    }
}

public class TopicDto {
    public int TopicId { get; set; }
    public string TopicTypeString { get; set; } = string.Empty;
    public TopicType TopicType { get; set; }
    public TopicBankFlag TopicBankFlag { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public string TopicText { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public string TopicSubjectName { get; set; } = string.Empty;
    public int TopicSubjectId { get; set; }
    public IEnumerable<TopicOptionDto> TopicOptions { get; set; } = [];
}

public class TopicOptionDto {
    public int TopicOptionId { get; set; }
    public int TopicId { get; set; }
    public char Code { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class TopicCreate {
    public TopicBankFlag TopicBankFlag { get; set; }
    [MaxLength(500)]
    public required string TopicText { get; set; }
    [MaxLength(256)]
    public required string CorrectAnswer { get; set; }
    [EnumDataType(typeof(TopicType), ErrorMessage = "无效的枚举值")]
    public required TopicType TopicType { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel DifficultyLevel { get; set; }
    public required int TopicSubjectId { get; set; }
    public required string Analysis { get; set; }
    public IEnumerable<TopicOptionCreate> TopicOptions { get; set; } = [];
}

public class TopicUpdate {
    [MaxLength(500)]
    public string? TopicText { get; set; }
    [MaxLength(256)]
    public string? CorrectAnswer { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel DifficultyLevel { get; set; }
    public string? Analysis { get; set; }
    public int? TopicSubjectId { get; set; }
    public IEnumerable<TopicOptionCreate> TopicOptions { get; set; } = [];
}

public class TopicOptionCreate {
    public int TopicId { get; set; }
    public char Code { get; set; }
    public string Text { get; set; } = string.Empty;
}
