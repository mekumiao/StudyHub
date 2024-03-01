using System.Collections.ObjectModel;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Helpers;
using StudyHub.WPF.Models;
using StudyHub.WPF.Services;
using StudyHub.WPF.ViewModels.Dialogs;
using StudyHub.WPF.Views.Dialogs;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

internal class TopicListViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<TopicDto, NotifyCheckedChanged<TopicDto>>()
            .Map(dest => dest.Model, src => src);
    }
}

public partial class TopicListViewModel(
    TopicSubjectOptionService topicSubjectOptionService,
    TopicService topicService,
    IMapper mapper,
    NotificationService notificationService) : ObservableObject, INavigationAware {
    private bool _isInitialized;
    public static IReadOnlyList<LabelValueOption> LevelOptions => EnumerationOptionService.GetDifficultyLevelOptionsWithDefault();
    public static IReadOnlyList<LabelValueOption> TopicTypeOptions => EnumerationOptionService.GetTopicTypeOptionsWithDefault();

    protected virtual TopicBankFlag TopicBankFlag => TopicBankFlag.Evaluation;

    public void OnNavigatedFrom() {
    }

    public async void OnNavigatedTo() {
        if (_isInitialized is false) {
            await LoadTopicSubjectsCommand.ExecuteAsync(null);
            await LoadTopicsCommand.ExecuteAsync(null);
        }
    }

    [ObservableProperty]
    private LabelValueOption[] _subjects = [];
    [ObservableProperty]
    private int _selectedSubjectId;
    [ObservableProperty]
    private int _level;
    [ObservableProperty]
    private string? _keyword;
    [ObservableProperty]
    private int _topicType;
    [ObservableProperty]
    private int _total;
    [ObservableProperty]
    private int _currentPage = 1;
    [ObservableProperty]
    private int _countPerPage = 20;

    private bool CanLoadTopics { get; set; }

    partial void OnSelectedSubjectIdChanged(int value) {
        if (CanLoadTopics) {
            LoadTopicsCommand.ExecuteAsync(null);
        }
    }

    partial void OnLevelChanged(int value) {
        if (CanLoadTopics) {
            LoadTopicsCommand.ExecuteAsync(null);
        }
    }

    partial void OnTopicTypeChanged(int value) {
        if (CanLoadTopics) {
            LoadTopicsCommand.ExecuteAsync(null);
        }
    }

    partial void OnCurrentPageChanged(int value) {
        if (CanLoadTopics) {
            LoadTopicsCommand.ExecuteAsync(null);
        }
    }

    partial void OnCountPerPageChanged(int value) {
        if (CanLoadTopics) {
            LoadTopicsCommand.ExecuteAsync(null);
        }
    }

    private Paging GetPaging() {
        return Paging.FromPageNumber(CurrentPage, CountPerPage);
    }

    public SupportsSelectAllOfDataContext<TopicDto> TopicDataContext { get; } = new();

    [RelayCommand]
    private async Task OnRefreshAsync() {
        await OnLoadTopicSubjectsAsync();
        await OnLoadTopicsAsync();
    }

    [RelayCommand]
    private async Task OnLoadTopicSubjectsAsync() {
        CanLoadTopics = false;
        LoadTopicsCommand.NotifyCanExecuteChanged();
        try {
            Subjects = await topicSubjectOptionService.GetOptionsWithDefaultAsync();
            SelectedSubjectId = 0;
            _isInitialized = true;
        }
        finally {
            CanLoadTopics = true;
            LoadTopicsCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoadTopics))]
    private async Task OnLoadTopicsAsync() {
        var filter = new TopicFilter {
            TopicBankFlag = TopicBankFlag,
            TopicSubjectId = SelectedSubjectId,
            DifficultyLevel = (DifficultyLevel)Level,
            TopicType = (TopicType)TopicType,
            TopicTextOrId = Keyword,
        };
        var result = await topicService.GetListAsync(filter, GetPaging());
        if (result.IsSuccess is false) {
            return;
        }
        Total = result.Result.Total;
        var notifies = mapper.Map<NotifyCheckedChanged<TopicDto>[]>(result.Result.Items);
        TopicDataContext.Items = new ObservableCollection<NotifyCheckedChanged<TopicDto>>(notifies);
    }

    [RelayCommand]
    private async Task OnImportTopicFromExcelAsync() {
        var openFileDialog = new OpenFileDialog {
            Filter = "Excel|*.xlsx"
        };
        var result = openFileDialog.ShowDialog();
        if (result == false) return;

        using var scope = App.CreateAsyncScope();
        var topicService = scope.ServiceProvider.GetRequiredService<TopicService>();
        var serviceResult = await topicService.ImportTopicFromExcelFileNameAsync(openFileDialog.FileName, TopicBankFlag);
        if (serviceResult.IsSuccess is false) {
            var messageBox = new MessageBox {
                Title = "导入题目错误",
                Content = serviceResult.Message,
                CloseButtonText = "关闭",
            };
            await messageBox.ShowDialogAsync();
        }
        await OnRefreshAsync();
    }

    [RelayCommand]
    private async Task OnExportExcelFromTopicIdsAsync() {
        var topicIds = TopicDataContext.GetCheckedItems().Select(v => v.Model.TopicId).ToArray();
        if (topicIds.Length == 0) {
            notificationService.ShowInfo("没有任何选中项");
            return;
        }

        var openFileDialog = new SaveFileDialog {
            Filter = "Excel|*.xlsx",
            //CreatePrompt = true,
            DefaultExt = ".xlsx",
        };
        var result = openFileDialog.ShowDialog();
        if (result == false) return;

        using var scope = App.CreateAsyncScope();
        var topicService = scope.ServiceProvider.GetRequiredService<TopicService>();
        var serviceResult = await topicService.ExportExcelFromTopicIdsAsync(openFileDialog.FileName, topicIds);
        if (serviceResult.IsSuccess is false) {
            var messageBox = new MessageBox {
                Title = "导出题目错误",
                Content = serviceResult.Message,
                CloseButtonText = "关闭",
            };
            await messageBox.ShowDialogAsync();
        }
        else {
            notificationService.ShowSuccess("导出成功");
        }
    }

    /// <summary>
    /// 清空未交卷的答题记录
    /// </summary>
    /// <returns></returns>
    private static async Task CleanUnSubmissionAnswerRecordsAsync() {
        var answerRecordService = App.GetRequiredService<AnswerRecordService>();
        var filter = new AnswerRecordFilter {
            IsSubmission = false,
        };
        await answerRecordService.CleanAsync(filter);
    }

    [RelayCommand]
    private async Task OnDeleteSelected() {
        var selectedIds = TopicDataContext.GetCheckedItems().Select(v => v.Model.TopicId).ToArray();
        if (selectedIds.Length == 0) {
            notificationService.ShowInfo("没有任何选中项");
            return;
        }

        var result = await MessageBoxHelper.Delete.ShowDialogAsync();
        if (result is MessageBoxResult.Primary) {
            await CleanUnSubmissionAnswerRecordsAsync();
            using var scope = App.CreateAsyncScope();
            var topicService = scope.ServiceProvider.GetRequiredService<TopicService>();
            if (selectedIds.Length == 1) {
                var serviceResult = await topicService.DeleteAsync(selectedIds[0]);
                if (serviceResult.IsSuccess is false) {
                    notificationService.ShowWarning(serviceResult.Message);
                    return;
                }
            }
            else {
                await topicService.DeleteItemsAsync(selectedIds);
            }
            await OnLoadTopicsAsync();
        }
    }

    [RelayCommand]
    private async Task OnCreateAsync() {
        using var scope = App.CreateAsyncScope();
        var model = scope.ServiceProvider.GetRequiredService<TopicCreateViewModel>();
        model.TopicBankFlag = TopicBankFlag;
        var dialog = scope.ServiceProvider.GetRequiredService<TopicCreateDialog>();
        var result = await dialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            await OnLoadTopicsAsync();
        }
    }

    [RelayCommand]
    private async Task OnEditingAsync(object? paramer) {
        if (paramer is not NotifyCheckedChanged<TopicDto> item) return;
        using var scope = App.CreateAsyncScope();
        var model = scope.ServiceProvider.GetRequiredService<TopicCreateViewModel>();
        mapper.Map(item.Model, model);
        var dialog = scope.ServiceProvider.GetRequiredService<TopicCreateDialog>();
        var result = await dialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            await OnLoadTopicsAsync();
        }
    }
}
