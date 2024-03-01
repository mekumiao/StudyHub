using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.Services;

public class NotificationService(ISnackbarService snackbarService) {
    private readonly TimeSpan _notificationTimeout = TimeSpan.FromSeconds(3);

    public void ShowPrimary(string content) {
        snackbarService.Show(
                "摘要",
                content,
                ControlAppearance.Primary,
                new SymbolIcon(SymbolRegular.Info24, filled: false),
                _notificationTimeout);
    }

    public void ShowSuccess(string content) {
        snackbarService.Show(
                "成功",
                content,
                ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.CheckmarkCircle24, filled: false),
                _notificationTimeout);
    }

    public void ShowInfo(string content) {
        snackbarService.Show(
                "信息",
                content,
                ControlAppearance.Info,
                new SymbolIcon(SymbolRegular.Info24, filled: false),
                _notificationTimeout);
    }

    public void ShowWarning(string content) {
        snackbarService.Show(
                "警告",
                content,
                ControlAppearance.Caution,
                new SymbolIcon(SymbolRegular.Warning24, filled: false),
                _notificationTimeout);
    }

    public void ShowError(string content) {
        snackbarService.Show(
               "错误",
               content,
               ControlAppearance.Caution,
               new SymbolIcon(SymbolRegular.ErrorCircle24, filled: false),
               _notificationTimeout);
    }

    public void ShowDanger(string content) {
        snackbarService.Show(
               "危险",
               content,
               ControlAppearance.Danger,
               new SymbolIcon(SymbolRegular.ShieldError24, filled: false),
               _notificationTimeout);
    }
}
