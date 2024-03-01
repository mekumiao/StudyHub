using StudyHub.WPF.ViewModels.Dialogs;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Dialogs;

public partial class SelectTopicSubjectDialog : INavigableView<SelectTopicSubjectViewModel> {
    public SelectTopicSubjectViewModel ViewModel { get; }

    public SelectTopicSubjectDialog(
        IContentDialogService contentDialogService,
        SelectTopicSubjectViewModel viewModel) : base(contentDialogService.GetContentPresenter()) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
