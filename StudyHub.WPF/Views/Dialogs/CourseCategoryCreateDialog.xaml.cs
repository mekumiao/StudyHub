using StudyHub.WPF.ViewModels.Dialogs;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Dialogs;

public partial class CourseCategoryCreateDialog : INavigableView<CourseCategoryCreateViewModel> {
    public CourseCategoryCreateViewModel ViewModel { get; }

    public CourseCategoryCreateDialog(
        IContentDialogService contentDialogService,
        CourseCategoryCreateViewModel viewModel) : base(contentDialogService.GetContentPresenter()) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void OnOpened(ContentDialog sender, RoutedEventArgs args) {
        NameTextBox.Focus();
    }

    protected override void OnButtonClick(ContentDialogButton button) {
        if (button is ContentDialogButton.Primary) {
            ViewModel.SaveCommand.Execute(null);
            if (ViewModel.IsError || ViewModel.HasErrors) return;
        }
        base.OnButtonClick(button);
    }
}
