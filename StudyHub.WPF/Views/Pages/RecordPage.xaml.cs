using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class RecordPage : INavigableView<RecordViewModel> {
    public RecordViewModel ViewModel { get; }

    public RecordPage(RecordViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void OnListViewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        ViewModel.RouteToAnswerPageCommand.Execute(null);
    }
}
