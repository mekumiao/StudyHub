using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class IncorrectlyTopicExplorerPage : INavigableView<IncorrectlyTopicExplorerViewModel> {
    public IncorrectlyTopicExplorerViewModel ViewModel { get; }

    public IncorrectlyTopicExplorerPage(IncorrectlyTopicExplorerViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
