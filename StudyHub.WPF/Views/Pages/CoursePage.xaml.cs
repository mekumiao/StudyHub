using System.Windows.Controls;

using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class CoursePage : Page, INavigableView<CourseViewModel> {
    public CourseViewModel ViewModel { get; }

    public CoursePage(CourseViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }
}
