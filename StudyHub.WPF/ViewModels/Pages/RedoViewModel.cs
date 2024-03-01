using System.Collections.ObjectModel;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Services;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public class RedoRouteData {
    public TopicSubjectDto? TopicSubject { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
}

public partial class RedoViewModel(
    IMapper mapper,
    NotificationService notificationService,
    RedoRouteData redoRouteData,
    INavigationService navigationService) : ObservableObject, INavigationAware {
    public void OnNavigatedFrom() {
        SelectedAnswerRecord = null;
        AnswerRecords.Clear();
    }

    public async void OnNavigatedTo() {
        Level = redoRouteData.DifficultyLevel;
        SubjectName = redoRouteData.TopicSubject?.Name ?? string.Empty;
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
        if (redoRouteData.TopicSubject is null) return;

        var create = mapper.Map<RedoingRecordCreate>(this);
        create = create with {
            TopicSubjectId = redoRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = redoRouteData.DifficultyLevel,
        };
        using var scope = App.CreateAsyncScope();
        var answerRecordService = scope.ServiceProvider.GetRequiredService<AnswerRecordService>();
        var serviceResult = await answerRecordService.CreateAsync(create);
        if (serviceResult.IsSuccess is false) {
            notificationService.ShowInfo(serviceResult.Message);
            return;
        }

        var routeData = App.GetRequiredService<AnswerRouteData>();
        routeData.IsShowSubmissionButton = true;
        routeData.IsShowSwitchAnalysisButton = true;
        routeData.IsAutomaticNextTopic = true;
        routeData.AnswerRecord = serviceResult.Result;

        navigationService.NavigateWithHierarchy(typeof(AnswerPage));
    }

    [RelayCommand]
    private async Task OnLoadAnswerRecordsAsync() {
        if (redoRouteData.TopicSubject is null) return;

        var answerRecordService = App.GetRequiredService<AnswerRecordService>();
        var filter = new AnswerRecordFilter {
            TopicSubjectId = redoRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = redoRouteData.DifficultyLevel,
            IsSubmission = true,
            AnswerRecordType = AnswerRecordType.Redo,
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

        var answerRecordService = App.GetRequiredService<AnswerRecordService>();
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
        if (redoRouteData.TopicSubject is null) return;

        var scope = App.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StudyHubDbContext>();
        var answerRecordService = scope.ServiceProvider.GetRequiredService<AnswerRecordService>();
        var filter = new AnswerRecordFilter {
            TopicSubjectId = redoRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = redoRouteData.DifficultyLevel,
            AnswerRecordType = AnswerRecordType.Redo,
        };
        using var trans = await dbContext.Database.BeginTransactionAsync();
        await answerRecordService.CleanAsync(filter);
        await answerRecordService.ResetValueOfIsRedoCorrectlyField(redoRouteData.TopicSubject.TopicSubjectId, redoRouteData.DifficultyLevel);
        await trans.CommitAsync();
        AnswerRecords.Clear();
        await OnLoadAnswerRecordsAsync();
    }
}
