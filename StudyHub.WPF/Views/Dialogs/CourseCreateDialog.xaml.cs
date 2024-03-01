using StudyHub.WPF.ViewModels.Dialogs;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Dialogs;

public partial class CourseCreateDialog : INavigableView<CourseCreateViewModel> {
    public CourseCreateViewModel ViewModel { get; }

    public CourseCreateDialog(IContentDialogService contentDialogService, CourseCreateViewModel viewModel) : base(contentDialogService.GetContentPresenter()) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    protected override void OnButtonClick(ContentDialogButton button) {
        if (button is ContentDialogButton.Primary) {
            ViewModel.SaveCommand.Execute(null);
            if (ViewModel.IsSaved is false) return;
        }
        base.OnButtonClick(button);
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        ViewModel.LoadCommand.ExecuteAsync(null);
        ViewModel.SavedEvent += OnSaved;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) {
        ViewModel.SavedEvent -= OnSaved;
    }

    private void OnSaved(object? sender, EventArgs e) {
        base.OnButtonClick(ContentDialogButton.Primary);
    }

    private void OnOpened(ContentDialog sender, RoutedEventArgs args) {
        TitleTextBox.Focus();
    }

    private void OnCoverMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        ViewModel.SelectCoverOnExplorerCommand.Execute(null);
    }

    //private void OnOpened(ContentDialog sender, RoutedEventArgs args) {
    //    NameTextBox.Focus();
    //}

    //protected override void OnButtonClick(ContentDialogButton button) {
    //    if (button is ContentDialogButton.Primary) {
    //        ViewModel.SaveCommand.Execute(null);
    //        if (ViewModel.IsError || ViewModel.HasErrors) return;
    //    }
    //    base.OnButtonClick(button);
    //}
}
