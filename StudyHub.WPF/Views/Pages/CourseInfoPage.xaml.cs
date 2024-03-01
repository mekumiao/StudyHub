using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.Views.Pages;

public partial class CourseInfoPage : Page, INavigableView<CourseInfoViewModel> {
    public CourseInfoViewModel ViewModel { get; }

    public CourseInfoPage(CourseInfoViewModel viewModel) {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    private void OnListViewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
        ViewModel.RouteToCoursePlayerPageCommand.Execute(null);
    }
}
