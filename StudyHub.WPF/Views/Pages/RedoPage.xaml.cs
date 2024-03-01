using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class RedoPage : INavigableView<RedoViewModel> {
    public RedoViewModel ViewModel { get; }

    public RedoPage(RedoViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void OnListViewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        ViewModel.RouteToAnswerPageUseRecordCommand.Execute(this);
    }
}
