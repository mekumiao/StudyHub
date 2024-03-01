using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class ParameterSettingPage : INavigableView<ParameterSettingViewModel> {
    public ParameterSettingViewModel ViewModel { get; }

    public ParameterSettingPage(ParameterSettingViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
