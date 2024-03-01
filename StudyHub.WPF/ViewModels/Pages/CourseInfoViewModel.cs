using System.Collections.ObjectModel;

using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public class CourseInfoRouteData {
    public int? CourseId { get; set; }
}

public partial class CourseInfoViewModel(
    INavigationService navigationService,
    CourseService courseService,
    CourseInfoRouteData courseInfoRouteData) : ObservableObject, INavigationAware {

    public void OnNavigatedFrom() {
    }

    public async void OnNavigatedTo() {
        await LoadCourseCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private CourseDto? _course;
    [ObservableProperty]
    private ObservableCollection<CourseSectionDto> _courseSections = [];
    [ObservableProperty]
    private int? _selectedSectionId;

    [RelayCommand]
    private async Task OnLoadCourseAsync() {
        if (courseInfoRouteData.CourseId is null) {
            return;
        }
        var result = await courseService.GetEntityByIdAsync(courseInfoRouteData.CourseId.Value);
        if (result.IsSuccess is false) {
            return;
        }
        Course = result.Result;
        CourseSections = new ObservableCollection<CourseSectionDto>(Course.CourseSections);
    }

    [RelayCommand]
    private void OnRouteToCoursePlayerPage() {
        var routeData = App.GetRequiredService<CoursePlayerRouteData>();
        routeData.SelectedSectionId = SelectedSectionId;
        routeData.Course = Course;
        navigationService.NavigateWithHierarchy(typeof(CoursePlayerPage));
    }
}
