using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Service;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;
using Wpf.Ui;
using StudyHub.WPF.Views.Pages;
using StudyHub.Storage.Entities;

namespace StudyHub.WPF.ViewModels.Pages;

public class RecordRouteData {
    public TopicSubjectDto? TopicSubject { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
}

public partial class RecordViewModel(AnswerRecordService answerRecordService, INavigationService navigationService, RecordRouteData recordRouteData) : ObservableObject, INavigationAware {
    public void OnNavigatedFrom() {
        SelectedAnswerRecord = null;
        AnswerRecords.Clear();
    }

    public async void OnNavigatedTo() {
        await LoadAnswerRecordsCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private ObservableCollection<AnswerRecordDto> _answerRecords = [];
    [ObservableProperty]
    private AnswerRecordDto? _selectedAnswerRecord;

    [RelayCommand]
    private async Task OnLoadAnswerRecordsAsync() {
        if (recordRouteData.TopicSubject is null) return;
        var filter = new AnswerRecordFilter {
            IsExamination = true,
            IsSubmission = true,
            TopicSubjectId = recordRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = recordRouteData.DifficultyLevel,
        };
        var result = await answerRecordService.GetListAsync(filter, Paging.None);
        if (result.IsSuccess is false) {
            return;
        }
        await App.GetMainWindowFromService().Dispatcher.InvokeAsync(() => {
            foreach (var item in result.Result.Items) {
                AnswerRecords.Add(item);
            }
        });
    }

    /// <summary>
    /// 路由到答题页
    /// </summary>
    [RelayCommand]
    private async Task OnRouteToAnswerPageAsync() {
        if (SelectedAnswerRecord is null) {
            return;
        }

        var result = await answerRecordService.GetEntityByIdAsync(SelectedAnswerRecord.AnswerRecordId);
        if (result.IsSuccess is false) {
            return;
        }

        var routeData = App.GetRequiredService<AnswerRouteData>();
        routeData.IsShowSwitchAnalysisButton = true;
        routeData.IsShowSubmissionButton = false;
        routeData.IsAutomaticNextTopic = false;
        routeData.OnlyDisplayIncorrectInBrowse = false;
        routeData.AnswerRecord = result.Result;

        navigationService.NavigateWithHierarchy(typeof(AnswerPage));
    }

    [RelayCommand]
    private async Task OnCleanAnswerRecordAsync() {
        if (recordRouteData.TopicSubject is null) return;
        var filter = new AnswerRecordFilter {
            TopicSubjectId = recordRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = recordRouteData.DifficultyLevel,
            IsExamination = true,
        };
        await answerRecordService.CleanAsync(filter);
        SelectedAnswerRecord = null;
        AnswerRecords.Clear();
        await OnLoadAnswerRecordsAsync();
    }
}
