using System.Windows.Controls;

using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class CourseListPage : INavigableView<CourseListViewModel> {
    public CourseListViewModel ViewModel { get; }

    public CourseListPage(CourseListViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        if (ItemsControl.ContainerFromElement((Wpf.Ui.Controls.DataGrid)sender, e.OriginalSource as DependencyObject) is DataGridRow row && !row.IsEditing && !row.IsNewItem) {
            ViewModel.EditingCommand.Execute(row.DataContext);
        }
    }
}
