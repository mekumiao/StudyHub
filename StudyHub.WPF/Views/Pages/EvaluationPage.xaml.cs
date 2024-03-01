using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class EvaluationPage : INavigableView<EvaluationViewModel> {
    public EvaluationViewModel ViewModel { get; }

    public EvaluationPage(EvaluationViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void OnListViewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        ViewModel.RouteToAnswerPageUseRecordCommand.Execute(this);
    }
}
