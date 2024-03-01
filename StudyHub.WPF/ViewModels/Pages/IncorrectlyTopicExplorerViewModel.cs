using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Services;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public partial class IncorrectlyTopicExplorerViewModel(
    INavigationService navigationService,
    NotificationService notificationService,
    TopicSubjectOptionService topicSubjectOptionService,
    AnswerRecordService answerRecordService) : ObservableObject, INavigationAware {
    public static IReadOnlyList<LabelValueOption> LevelOptions => EnumerationOptionService.GetDifficultyLevelOptionsWithDefault();
    public static IReadOnlyList<LabelValueOption> TopicTypeOptions => EnumerationOptionService.GetTopicTypeOptionsWithDefault();

    public void OnNavigatedFrom() {
    }

    public async void OnNavigatedTo() {
        await LoadSubjectOptionsCommand.ExecuteAsync(null);
        await LoadAnswerRecordCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private LabelValueOption[] _topicSubjects = [];
    [ObservableProperty]
    private int _subjectId;
    [ObservableProperty]
    private int _level;
    [ObservableProperty]
    private int _topicType;
    [ObservableProperty]
    private int _recordCount;
    private AnswerRecordItemDto[]? _answerRecordItems;

    partial void OnLevelChanged(int value) {
        LoadAnswerRecordCommand.Execute(null);
    }

    partial void OnSubjectIdChanged(int value) {
        LoadAnswerRecordCommand.Execute(null);
    }

    partial void OnTopicTypeChanged(int value) {
        LoadAnswerRecordCommand.Execute(null);
    }

    /// <summary>
    /// 刷新选中的科目ID
    /// </summary>
    private void RefreshSubjectId() {
        var oldSubjectId = SubjectId;
        SubjectId = 0; // 重置到0，让ComboBox的SelectedValue值变更，带动SelectedItem变更，防止ComboBox标红
        if (TopicSubjects.Any(v => v.Id == oldSubjectId)) {
            SubjectId = oldSubjectId;
        }
    }

    [RelayCommand]
    private async Task OnLoadSubjectOptionsAsync() {
        TopicSubjects = [.. await topicSubjectOptionService.GetOptionsWithDefaultAsync()];
        RefreshSubjectId();
    }

    [RelayCommand]
    private async Task OnLoadAnswerRecordAsync() {
        var filter = new AnswerRecordItemFilter {
            TopicSubjectId = SubjectId,
            DifficultyLevel = (DifficultyLevel)Level,
            TopicType = (TopicType)TopicType,
            IsIncorrectly = true,
        };
        var result = await answerRecordService.GetAnswerRecordItemAsync(filter, Paging.None);
        if (result.IsSuccess) {
            _answerRecordItems = result.Result;
            RecordCount = _answerRecordItems.Length;
        }
        else {
            _answerRecordItems = default;
            RecordCount = 0;
        }
    }

    [RelayCommand]
    private void OnRouteToAnswerPage() {
        if (_answerRecordItems is null || RecordCount <= 0) {
            notificationService.ShowInfo("没有任何错题");
            return;
        }
        var routeData = App.GetRequiredService<AnswerRouteData>();
        routeData.IsShowSubmissionButton = false;
        routeData.IsShowSwitchAnalysisButton = true;
        routeData.IsAutomaticNextTopic = false;
        routeData.OnlyDisplayIncorrectInBrowse = true;
        routeData.AnswerRecord = new AnswerRecordDto {
            AnswerRecordItems = _answerRecordItems,
        };
        navigationService.NavigateWithHierarchy(typeof(AnswerPage));
    }
}
