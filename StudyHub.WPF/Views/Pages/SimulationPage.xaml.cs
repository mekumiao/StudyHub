using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class SimulationPage : INavigableView<SimulationViewModel> {
    public SimulationViewModel ViewModel { get; }

    public SimulationPage(SimulationViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
