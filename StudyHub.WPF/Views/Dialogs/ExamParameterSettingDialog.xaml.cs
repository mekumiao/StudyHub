using StudyHub.WPF.ViewModels.Dialogs;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Dialogs;

public partial class ExamParameterSettingDialog : INavigableView<ExamParameterSettingViewModel> {
    public ExamParameterSettingViewModel ViewModel { get; }

    public ExamParameterSettingDialog(
        ExamParameterSettingViewModel viewModel,
        IContentDialogService contentDialogService) : base(contentDialogService.GetContentPresenter()) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void OnOpened(ContentDialog sender, RoutedEventArgs args) {
        ViewModel.LoadCommand.Execute(null);
    }

    protected override void OnButtonClick(ContentDialogButton button) {
        if (button is ContentDialogButton.Primary) {
            ViewModel.SaveCommand.Execute(null);
        }
        base.OnButtonClick(button);
    }
}
