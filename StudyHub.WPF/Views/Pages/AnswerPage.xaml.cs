using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class AnswerPage : INavigableView<AnswerViewModel> {
    public AnswerViewModel ViewModel { get; }

    public AnswerPage(AnswerViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void TopicSheet_CountdownEnd(object sender, EventArgs e) {
        ViewModel.SubmissionCommand.Execute(true);
    }
}
