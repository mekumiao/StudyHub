using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Services;
using StudyHub.WPF.Views.Windows;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public class SimulationRouteData {
    public TopicSubjectDto? TopicSubject { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
}

public partial class SimulationViewModel(
    SimulationRouteData simulationRouteData,
    NotificationService notificationService,
    IMapper mapper) : ObservableObject, INavigationAware {
    public void OnNavigatedFrom() {
    }

    public void OnNavigatedTo() {
        Level = simulationRouteData.DifficultyLevel;
        SubjectName = simulationRouteData.TopicSubject?.Name ?? string.Empty;
    }
    public static IReadOnlyList<int> Scores => [1, 2, 3, 5];

    [ObservableProperty]
    private DifficultyLevel _level;
    [ObservableProperty]
    private string _subjectName = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _singleCount = 10;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _multipleCount = 10;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _trueFalseCount = 10;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _fillCount = 10;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _singleScore = 1;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _multipleScore = 2;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _trueFalseScore = 2;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _fillScore = 5;
    [ObservableProperty]
    private int _totalMinutes = 60;

    public int TotalScore =>
        SingleCount * SingleScore +
        MultipleCount * MultipleScore +
        TrueFalseCount * TrueFalseScore +
        FillCount * FillScore;

    [RelayCommand]
    private async Task OnRouteToAnswerPageAsync() {
        if (simulationRouteData.TopicSubject is null) return;

        var create = mapper.Map<SimulationRecordCreate>(this);
        create = create with {
            TopicSubjectId = simulationRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = simulationRouteData.DifficultyLevel,
            DurationSeconds = TotalMinutes * 60,
        };

        using var scope = App.CreateAsyncScope();
        var answerRecordService = scope.ServiceProvider.GetRequiredService<AnswerRecordService>();
        var result = await answerRecordService.CreateAsync(create);
        if (result.IsSuccess is false) {
            notificationService.ShowInfo(result.Message);
            return;
        }

        var routeData = App.GetRequiredService<AnswerRouteData>();
        mapper.Map(simulationRouteData, routeData);
        mapper.Map(this, routeData);

        routeData.IsShowSubmissionButton = true;
        routeData.IsShowSwitchAnalysisButton = false;
        routeData.IsAutomaticNextTopic = true;
        routeData.AnswerRecord = result.Result;

        App.GetRequiredService<AnswerWindow>().FullScree();
    }
}
