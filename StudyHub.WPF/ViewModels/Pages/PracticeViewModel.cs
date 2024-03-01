using System.Collections.ObjectModel;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Services;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public class PracticeRouteData {
    public TopicSubjectDto? TopicSubject { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
}

public partial class PracticeViewModel(
    IMapper mapper,
    PracticeRouteData practiceRouteData,
    INavigationService navigationService,
    NotificationService notificationService,
    AnswerRecordService answerRecordService) : ObservableObject, INavigationAware {
    public void OnNavigatedFrom() {
        SelectedAnswerRecord = null;
        AnswerRecords.Clear();
    }

    public async void OnNavigatedTo() {
        Level = practiceRouteData.DifficultyLevel;
        SubjectName = practiceRouteData.TopicSubject?.Name ?? string.Empty;
        await LoadAnswerRecordsCommand.ExecuteAsync(null);
    }

    [ObservableProperty]
    private DifficultyLevel _level;
    [ObservableProperty]
    private string _subjectName = string.Empty;
    [ObservableProperty]
    private int _singleCount = 10;
    [ObservableProperty]
    private int _multipleCount = 10;
    [ObservableProperty]
    private int _trueFalseCount = 10;
    [ObservableProperty]
    private int _fillCount = 10;
    [ObservableProperty]
    private ObservableCollection<AnswerRecordDto> _answerRecords = [];
    [ObservableProperty]
    private AnswerRecordDto? _selectedAnswerRecord;
    [ObservableProperty]
    private bool _isExpanded;

    [RelayCommand]
    private async Task OnRouteToAnswerPageAsync() {
        if (practiceRouteData.TopicSubject is null) return;

        var create = mapper.Map<PracticeRecordCreate>(this);
        create = create with {
            TopicSubjectId = practiceRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = practiceRouteData.DifficultyLevel,
        };

        using var scope = App.CreateAsyncScope();
        var answerRecordService = scope.ServiceProvider.GetRequiredService<AnswerRecordService>();
        var result = await answerRecordService.CreateAsync(create);
        if (result.IsSuccess is false) {
            notificationService.ShowInfo(result.Message);
            return;
        }

        var routeData = App.GetRequiredService<AnswerRouteData>();
        mapper.Map(practiceRouteData, routeData);
        mapper.Map(this, routeData);
        routeData.IsShowSubmissionButton = true;
        routeData.IsShowSwitchAnalysisButton = false;
        routeData.IsAutomaticNextTopic = true;
        routeData.AnswerRecord = result.Result;

        navigationService.NavigateWithHierarchy(typeof(AnswerPage));
    }

    [RelayCommand]
    private async Task OnLoadAnswerRecordsAsync() {
        if (practiceRouteData.TopicSubject is null) return;

        var answerRecordService = App.GetRequiredService<AnswerRecordService>();
        var filter = new AnswerRecordFilter {
            TopicSubjectId = practiceRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = practiceRouteData.DifficultyLevel,
            IsSubmission = true,
            AnswerRecordType = AnswerRecordType.Practice,
        };
        var result = await answerRecordService.GetListAsync(filter, Paging.None);
        if (result.IsSuccess is false) {
            AnswerRecords.Clear();
            IsExpanded = false;
            return;
        }
        IsExpanded = result.Result.Items.Length > 0;
        if (IsExpanded) {
            AnswerRecords = new(result.Result.Items);
        }
    }

    [RelayCommand]
    private async Task OnRouteToAnswerPageUseRecordAsync() {
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
        if (practiceRouteData.TopicSubject is null) return;

        var answerRecordService = App.GetRequiredService<AnswerRecordService>();
        var filter = new AnswerRecordFilter {
            TopicSubjectId = practiceRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = practiceRouteData.DifficultyLevel,
            AnswerRecordType = AnswerRecordType.Practice,
        };
        await answerRecordService.CleanAsync(filter);
        AnswerRecords.Clear();
        await OnLoadAnswerRecordsAsync();
    }
}
