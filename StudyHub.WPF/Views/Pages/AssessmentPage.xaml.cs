using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class AssessmentPage : INavigableView<AssessmentViewModel> {
    public AssessmentViewModel ViewModel { get; }

    public AssessmentPage(AssessmentViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
