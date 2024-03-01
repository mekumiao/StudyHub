using StudyHub.WPF.Views.Dialogs;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public partial class ParameterSettingViewModel : ObservableObject, INavigationAware {
    public void OnNavigatedFrom() {
    }

    public void OnNavigatedTo() {
    }

    [RelayCommand]
    private static async Task OnOpenExamParameterSettingDialogAsync() {
        var dialog = App.GetRequiredService<ExamParameterSettingDialog>();
        await dialog.ShowAsync();
    }
}
