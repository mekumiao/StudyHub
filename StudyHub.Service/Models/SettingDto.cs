namespace StudyHub.Service.Models;

public class SettingDto {
    public int SettingId { get; set; }
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string SettingType { get; set; } = string.Empty;
    public string SettingDescription { get; set; } = string.Empty;
}
