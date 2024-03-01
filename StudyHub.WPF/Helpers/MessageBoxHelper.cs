namespace StudyHub.WPF.Helpers;

public class MessageBoxHelper {
    public static MessageBox Delete => new() {
        Title = "删除确认",
        Content = "确认要删除吗？",
        IsPrimaryButtonEnabled = true,
        PrimaryButtonText = "确认",
        CloseButtonText = "取消",
    };

    public static MessageBox Message(string message) => new() {
        Title = "信息",
        Content = message,
        CloseButtonText = "取消",
    };
}
