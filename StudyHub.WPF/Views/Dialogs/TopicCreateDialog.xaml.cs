using StudyHub.WPF.ViewModels.Dialogs;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Dialogs;

public partial class TopicCreateDialog {
    public TopicCreateViewModel ViewModel { get; }

    public TopicCreateDialog(
        IContentDialogService contentDialogService,
        TopicCreateViewModel viewModel) : base(contentDialogService.GetContentPresenter()) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    protected override void OnButtonClick(ContentDialogButton button) {
        if (button is ContentDialogButton.Primary) {
            ViewModel.SaveCommand.Execute(null);
            if (ViewModel.IsError || ViewModel.HasErrors) return;
        }
        base.OnButtonClick(button);
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        ViewModel.LoadCommand.Execute(null);
    }

    private void OnOpened(ContentDialog sender, RoutedEventArgs args) {
        TopicTextBox.Focus();
    }
}
