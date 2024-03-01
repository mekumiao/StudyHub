using System.Collections.ObjectModel;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.WPF.Helpers;
using StudyHub.WPF.Models;
using StudyHub.WPF.Services;
using StudyHub.WPF.ViewModels.Dialogs;
using StudyHub.WPF.Views.Dialogs;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

internal class TopicSubjectListViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<TopicSubjectDto, NotifyCheckedChanged<TopicSubjectDto>>()
            .Map(dest => dest.Model, src => src);
    }
}

public partial class TopicSubjectListViewModel(
    TopicSubjectService topicSubjectService,
    IMapper mapper,
    AssessmentViewModelRouteData assessmentViewModelRouteData,
    NotificationService notificationService) : ObservableObject, INavigationAware {
    private bool _isInitialized;

    public void OnNavigatedFrom() {
    }

    public async void OnNavigatedTo() {
        if (_isInitialized is false) {
            await LoadCommand.ExecuteAsync(null);
        }
    }

    public SupportsSelectAllOfDataContext<TopicSubjectDto> TopicSubjectDataContext { get; } = new();

    [RelayCommand]
    private async Task OnRefreshAsync() {
        await OnLoadAsync();
    }

    [RelayCommand]
    private async Task OnLoadAsync() {
        var filter = new TopicSubjectFilter { };
        var result = await topicSubjectService.GetListAsync(filter, Paging.None);
        if (result.IsSuccess) {
            var notifies = mapper.Map<NotifyCheckedChanged<TopicSubjectDto>[]>(result.Result.Items);
            TopicSubjectDataContext.Items = new ObservableCollection<NotifyCheckedChanged<TopicSubjectDto>>(notifies);
            _isInitialized = true;
        }
    }

    [RelayCommand]
    private async Task OnDeleteSelected() {
        var selectedIds = TopicSubjectDataContext.GetCheckedItems().Select(v => v.Model.TopicSubjectId).ToArray();
        if (selectedIds.Length == 0) {
            notificationService.ShowInfo("没有任何选中项");
            return;
        }

        var result = await MessageBoxHelper.Delete.ShowDialogAsync();
        if (result is MessageBoxResult.Primary) {
            using var scope = App.CreateAsyncScope();
            var topicSubjectService = scope.ServiceProvider.GetRequiredService<TopicSubjectService>();
            if (selectedIds.Length == 1) {
                var serviceResult = await topicSubjectService.DeleteAsync(selectedIds[0]);
                if (serviceResult.IsSuccess is false) {
                    notificationService.ShowWarning(serviceResult.Message);
                    return;
                }
            }
            else {
                await topicSubjectService.DeleteItemsAsync(selectedIds);
            }
            await OnLoadAsync();
            assessmentViewModelRouteData.NeedToRefresh = true;
        }
    }

    [RelayCommand]
    private async Task OnCreateAsync() {
        using var scope = App.CreateAsyncScope();
        var dialog = scope.ServiceProvider.GetRequiredService<TopicSubjectCreateDialog>();
        var result = await dialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            await OnLoadAsync();
            assessmentViewModelRouteData.NeedToRefresh = true;
        }
    }

    [RelayCommand]
    private async Task OnEditingAsync(object? paramer) {
        if (paramer is not NotifyCheckedChanged<TopicSubjectDto> item) return;
        using var scope = App.CreateAsyncScope();
        var model = scope.ServiceProvider.GetRequiredService<TopicSubjectCreateViewModel>();
        mapper.Map(item.Model, model);
        var dialog = scope.ServiceProvider.GetRequiredService<TopicSubjectCreateDialog>();
        var result = await dialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            await OnLoadAsync();
            assessmentViewModelRouteData.NeedToRefresh = true;
        }
    }
}
