using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class CheckResultPage : INavigableView<CheckResultViewModel> {
    public CheckResultViewModel ViewModel { get; }

    public CheckResultPage(CheckResultViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
