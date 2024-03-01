using System.Collections.ObjectModel;

using StudyHub.Service.Base;
using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public class CourseViewModelRouteData {
    public bool NeedToRefresh { get; set; }
}

public partial class CourseViewModel(
    INavigationService navigationService,
    CourseViewModelRouteData courseViewModelRouteData,
    CourseService courseService,
    CourseCategoryOptionService courseCategoryOptionService) : ObservableObject, INavigationAware {
    private bool _isInitialized;

    public void OnNavigatedFrom() {
    }

    public async void OnNavigatedTo() {
        if (courseViewModelRouteData.NeedToRefresh is true || _isInitialized is false) {
            await LoadCategoriesCommand.ExecuteAsync(null);
            await LoadCoursesCommand.ExecuteAsync(null);
            courseViewModelRouteData.NeedToRefresh = false;
        }
    }

    [ObservableProperty]
    private ObservableCollection<CourseDto> _courses = [];
    [ObservableProperty]
    private LabelValueOption[] _categories = [];
    [ObservableProperty]
    private int _selectedCategoryId;
    private bool CanLoadCourses { get; set; }

    partial void OnSelectedCategoryIdChanged(int value) {
        if (CanLoadCourses) {
            LoadCoursesCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task OnRefreshAsync() {
        await OnLoadCategoriesAsync();
        await OnLoadCoursesAsync();
    }

    [RelayCommand]
    private async Task OnLoadCategoriesAsync() {
        CanLoadCourses = false;
        LoadCoursesCommand.NotifyCanExecuteChanged();
        try {
            Categories = await courseCategoryOptionService.GetOptionsWithDefaultAsync();
            SelectedCategoryId = 0;
            _isInitialized = true;
        }
        finally {
            CanLoadCourses = true;
            LoadCoursesCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoadCourses))]
    private async Task OnLoadCoursesAsync() {
        var filter = new CourseFilter {
            CourseCategoryId = SelectedCategoryId,
        };
        var result = await courseService.GetListAsync(filter, Paging.None);
        if (result.IsSuccess) {
            Courses = new ObservableCollection<CourseDto>(result.Result.Items);
            _isInitialized = true;
        }
    }

    [RelayCommand]
    private void OnRouteToCourseInfoPage(object? obj) {
        if (obj is CourseDto course) {
            var courseInfoRouteData = App.GetRequiredService<CourseInfoRouteData>();
            courseInfoRouteData.CourseId = course.CourseId;
            navigationService.NavigateWithHierarchy(typeof(CourseInfoPage));
        }
    }
}
