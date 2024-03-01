namespace StudyHub.Service.Models;

public record SubmissionModel {
    public int AnswerRecordId { get; set; }
    public IEnumerable<SubmissionItemModel> SubmissionItems { get; set; } = [];
}

public record SubmissionItemModel {
    public int AnswerRecordItemId { get; set; }
    /// <summary>
    /// 答案:
    /// 1.单选题和多选题，多选题直接将答案拼接即可。如：ABC
    /// 2.判断题，0表示错，1表示对
    /// 3.填空题，直接填入文本
    /// 4.null表示未作答
    /// </summary>
    public string? AnswerText { get; set; }
}
