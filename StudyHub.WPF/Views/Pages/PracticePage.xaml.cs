using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class PracticePage : INavigableView<PracticeViewModel> {
    public PracticeViewModel ViewModel { get; }

    public PracticePage(PracticeViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void OnListViewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        ViewModel.RouteToAnswerPageUseRecordCommand.Execute(this);
    }
}
