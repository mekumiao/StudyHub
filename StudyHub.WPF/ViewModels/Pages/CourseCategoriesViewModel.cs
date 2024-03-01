using System.Collections.ObjectModel;

using StudyHub.Service.Base;
using StudyHub.Service;
using StudyHub.Service.Models;

using Wpf.Ui.Controls;
using StudyHub.WPF.Views.Dialogs;
using StudyHub.WPF.Models;
using MapsterMapper;
using Mapster;
using StudyHub.WPF.Services;
using StudyHub.WPF.Helpers;
using Microsoft.Extensions.DependencyInjection;
using StudyHub.WPF.ViewModels.Dialogs;

namespace StudyHub.WPF.ViewModels.Pages;

internal class CourseCategoriesViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<CourseCategoryDto, NotifyCheckedChanged<CourseCategoryDto>>()
            .Map(dest => dest.Model, src => src);
    }
}

public partial class CourseCategoriesViewModel(
    CourseViewModelRouteData courseViewModelRouteData,
    CourseCategoryService courseCategoryService,
    NotificationService notificationService,
    IMapper mapper) : ObservableObject, INavigationAware {
    private bool _isInitialized;

    public void OnNavigatedFrom() {
    }

    public async void OnNavigatedTo() {
        if (_isInitialized is false) {
            await LoadCommand.ExecuteAsync(null);
        }
    }

    public SupportsSelectAllOfDataContext<CourseCategoryDto> CategoryDataContext { get; } = new();

    [RelayCommand]
    private async Task OnRefreshAsync() {
        await OnLoadAsync();
    }

    [RelayCommand]
    private async Task OnLoadAsync() {
        var filter = new CourseCategoryFilter { };
        var result = await courseCategoryService.GetListAsync(filter, Paging.None);
        if (result.IsSuccess && result.Result.Items.Length > 0) {
            var notifies = mapper.Map<NotifyCheckedChanged<CourseCategoryDto>[]>(result.Result.Items);
            CategoryDataContext.Items = new ObservableCollection<NotifyCheckedChanged<CourseCategoryDto>>(notifies);
            _isInitialized = true;
            CategoryDataContext.Items.CollectionChanged += Items_CollectionChanged;
        }
        else {
            CategoryDataContext.Items.Clear();
        }
    }

    private async void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
            await courseCategoryService.SortingAsync(CategoryDataContext.Items.Select(v => v.Model.CourseCategoryId));
            courseViewModelRouteData.NeedToRefresh = true;
        }
    }

    [RelayCommand]
    private async Task OnDeleteSelected() {
        var selectedIds = CategoryDataContext.GetCheckedItems().Select(v => v.Model.CourseCategoryId).ToArray();
        if (selectedIds.Length == 0) {
            notificationService.ShowInfo("没有任何选中项");
            return;
        }

        var result = await MessageBoxHelper.Delete.ShowDialogAsync();
        if (result is MessageBoxResult.Primary) {
            using var scope = App.CreateAsyncScope();
            var courseCategoryService = scope.ServiceProvider.GetRequiredService<CourseCategoryService>();
            if (selectedIds.Length == 1) {
                var serviceResult = await courseCategoryService.DeleteAsync(selectedIds[0]);
                if (serviceResult.IsSuccess is false) {
                    notificationService.ShowWarning(serviceResult.Message);
                    return;
                }
            }
            else {
                await courseCategoryService.DeleteItemsAsync(selectedIds);
            }
            await OnLoadAsync();
            courseViewModelRouteData.NeedToRefresh = true;
        }
    }

    [RelayCommand]
    private async Task OnCreateAsync() {
        using var scope = App.CreateAsyncScope();
        var dialog = scope.ServiceProvider.GetRequiredService<CourseCategoryCreateDialog>();
        var result = await dialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            await OnLoadAsync();
            courseViewModelRouteData.NeedToRefresh = true;
        }
    }

    [RelayCommand]
    private async Task OnEditingAsync(object? paramer) {
        if (paramer is not NotifyCheckedChanged<CourseCategoryDto> item) return;
        using var scope = App.CreateAsyncScope();
        var model = scope.ServiceProvider.GetRequiredService<CourseCategoryCreateViewModel>();
        mapper.Map(item.Model, model);
        var dialog = scope.ServiceProvider.GetRequiredService<CourseCategoryCreateDialog>();
        var result = await dialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            await OnLoadAsync();
            courseViewModelRouteData.NeedToRefresh = true;
        }
    }
}
