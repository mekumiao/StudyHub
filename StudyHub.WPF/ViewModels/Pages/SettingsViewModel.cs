using System.Windows.Media;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject, INavigationAware {
    private bool _isInitialized = false;

    [ObservableProperty]
    private ApplicationTheme _currentApplicationTheme = ApplicationTheme.Unknown;

    public void OnNavigatedTo() {
        if (!_isInitialized) {
            InitializeViewModel();
        }
    }

    public void OnNavigatedFrom() { }

    partial void OnCurrentApplicationThemeChanged(ApplicationTheme oldValue, ApplicationTheme newValue) {
        ApplicationThemeManager.Apply(newValue);
    }

    private void InitializeViewModel() {
        CurrentApplicationTheme = ApplicationThemeManager.GetAppTheme();
        ApplicationThemeManager.Changed += OnThemeChanged;
        _isInitialized = true;
    }

    private void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent) {
        if (CurrentApplicationTheme != currentApplicationTheme) {
            CurrentApplicationTheme = currentApplicationTheme;
        }
    }
}
