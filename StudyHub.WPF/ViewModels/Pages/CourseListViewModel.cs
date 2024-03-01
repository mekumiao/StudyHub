using System.Collections.ObjectModel;
using System.IO;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.WPF.Models;
using StudyHub.WPF.Services;
using StudyHub.WPF.ViewModels.Dialogs;
using StudyHub.WPF.Views.Dialogs;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

internal class CourseListViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<CourseDto, NotifyCheckedChanged<CourseDto>>()
            .Map(dest => dest.Model, src => src);
    }
}

public partial class CourseListViewModel(
    IMapper mapper,
    CourseViewModelRouteData courseViewModelRouteData,
    CourseService courseService,
    CourseCategoryOptionService courseCategoryOptionService,
    NotificationService notificationService) : ObservableObject, INavigationAware {
    private bool _isInitialized;

    public void OnNavigatedFrom() {
    }

    public async void OnNavigatedTo() {
        if (_isInitialized is false) {
            await LoadCategoriesCommand.ExecuteAsync(null);
            await LoadCoursesCommand.ExecuteAsync(null);
        }
    }

    [ObservableProperty]
    private string _keyword = string.Empty;
    [ObservableProperty]
    private LabelValueOption[] _categories = [];
    [ObservableProperty]
    private int _selectedCategoryId;
    [ObservableProperty]
    private int _total;
    [ObservableProperty]
    private int _currentPage = 1;
    [ObservableProperty]
    private int _countPerPage = 20;

    partial void OnCurrentPageChanged(int value) {
        LoadCoursesCommand.ExecuteAsync(null);
    }

    partial void OnCountPerPageChanged(int value) {
        LoadCoursesCommand.ExecuteAsync(null);
    }

    private Paging GetPaging() {
        return Paging.FromPageNumber(CurrentPage, CountPerPage);
    }

    public SupportsSelectAllOfDataContext<CourseDto> CourseDataContext { get; } = new();

    partial void OnSelectedCategoryIdChanged(int value) {
        LoadCoursesCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task OnRefreshAsync() {
        await OnLoadCategoriesAsync();
        if (SelectedCategoryId == 0) {
            await OnLoadCoursesAsync();
        }
    }

    [RelayCommand]
    private async Task OnLoadCategoriesAsync() {
        Categories = await courseCategoryOptionService.GetOptionsWithDefaultAsync();
        SelectedCategoryId = 0;
        _isInitialized = true;
    }

    [RelayCommand]
    private async Task OnLoadCoursesAsync() {
        var filter = new CourseFilter {
            Keyword = Keyword,
            CourseCategoryId = SelectedCategoryId,
        };
        var result = await courseService.GetListAsync(filter, GetPaging());
        if (result.IsSuccess && result.Result.Items.Length > 0) {
            Total = result.Result.Total;
            var notifies = mapper.Map<NotifyCheckedChanged<CourseDto>[]>(result.Result.Items);
            CourseDataContext.Items = new ObservableCollection<NotifyCheckedChanged<CourseDto>>(notifies);
        }
        else {
            CourseDataContext.Items.Clear();
            Total = 0;
        }
    }

    [RelayCommand]
    private async Task OnSynchronizeCourseFromDirectory() {
        if (Directory.Exists(CourseService.CoursesRootDirectory)) {
            using var scope = App.CreateAsyncScope();
            var courseService = scope.ServiceProvider.GetRequiredService<CourseService>();
            await courseService.SynchronizeCourseToDbFromRootDirectoryAsync();
            await OnRefreshAsync();
            courseViewModelRouteData.NeedToRefresh = true;
        }
    }

    [RelayCommand]
    private async Task OnDeleteSelected() {
        var selectedIds = CourseDataContext.GetCheckedItems().Select(v => v.Model.CourseId).ToArray();
        if (selectedIds.Length == 0) {
            notificationService.ShowInfo("没有任何选中项");
            return;
        }

        var messageBox = new MessageBox {
            Title = "删除确认",
            Content = "确认要删除课程吗？",
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonAppearance = ControlAppearance.Danger,
            PrimaryButtonText = "标记删除",
            SecondaryButtonText = "彻底删除",
            CloseButtonText = "取消",
        };
        var result = await messageBox.ShowDialogAsync();
        if (result is MessageBoxResult.Primary or MessageBoxResult.Secondary) {
            using var scope = App.CreateAsyncScope();
            var courseService = scope.ServiceProvider.GetRequiredService<CourseService>();
            if (selectedIds.Length == 1) {
                var serviceResult = await courseService.DeleteAsync(selectedIds[0], result is MessageBoxResult.Secondary);
                if (serviceResult.IsSuccess is false) {
                    notificationService.ShowWarning(serviceResult.Message);
                    return;
                }
            }
            else {
                await courseService.DeleteItemsAsync(selectedIds, result is MessageBoxResult.Secondary);
            }
            await OnLoadCoursesAsync();
            courseViewModelRouteData.NeedToRefresh = true;
        }
    }

    [RelayCommand]
    private void OnOpenSelectedCourseInExplorer(object? paramer) {
        var course = paramer is NotifyCheckedChanged<CourseDto> courseNotify
            ? courseNotify.Model
            : (CourseDataContext.GetCheckedItems().FirstOrDefault()?.Model);
        if (course is null) {
            OpenDirectoryInExplorer(CourseService.CoursesRootDirectory);
        }
        else {
            OpenDirectoryInExplorer(Path.Combine(CourseService.CoursesRootDirectory, course.RelativePath));
        }

        static void OpenDirectoryInExplorer(string folder) {
            if (Directory.Exists(folder) is false) {
                Directory.CreateDirectory(folder);
            }
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }
    }

    [RelayCommand]
    private async Task OnCreateAsync() {
        using var scope = App.CreateAsyncScope();
        var dialog = scope.ServiceProvider.GetRequiredService<CourseCreateDialog>();
        var result = await dialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            await OnLoadCoursesAsync();
            courseViewModelRouteData.NeedToRefresh = true;
        }
    }

    [RelayCommand]
    private async Task OnEditingAsync(object? paramer) {
        if (paramer is not NotifyCheckedChanged<CourseDto> item) return;
        using var scope = App.CreateAsyncScope();
        var model = scope.ServiceProvider.GetRequiredService<CourseCreateViewModel>();
        mapper.Map(item.Model, model);
        var dialog = scope.ServiceProvider.GetRequiredService<CourseCreateDialog>();
        var result = await dialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            await OnLoadCoursesAsync();
            courseViewModelRouteData.NeedToRefresh = true;
        }
    }
}
