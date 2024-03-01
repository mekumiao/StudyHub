namespace StudyHub.Service.Models;

public class LabelValueOption {
    public readonly static LabelValueOption Default = new() { Id = 0, Text = "全部" };
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
