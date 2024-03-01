using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Threading;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Helpers;
using StudyHub.WPF.Services;
using StudyHub.WPF.ViewModels.Dialogs;
using StudyHub.WPF.Views.Windows;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

internal class EvaluationViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<AnswerRecordDto, AnswerRecordDtoNotifyChanged>()
            .Map(dest => dest.RemainingTime, src => src.StartTime!.Value.AddSeconds(src.DurationSeconds) - DateTime.UtcNow, should => should.StartTime.HasValue)
            .AfterMapping((src, dest) => {
                dest.AnswerRecordItems = dest.AnswerRecordItems.OrderBy(v => v.Order);
                dest.RemainingTime = dest.RemainingTime < TimeSpan.Zero ? TimeSpan.Zero : dest.RemainingTime;
            });

        config.NewConfig<ExamParameter, EvaluationViewModel>()
            .ConstructUsing(() => ExceptionHelper.ThrowCannotCreateEntityException<EvaluationViewModel>())
            .Map(dest => dest.Duration, src => TimeSpan.FromSeconds(src.DurationSecond));
    }
}

public class AnswerRecordDtoNotifyChanged : AnswerRecordDto, INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private TimeSpan _remainingTime = TimeSpan.Zero;
    /// <summary>
    /// 剩余的时间
    /// </summary>
    public TimeSpan RemainingTime {
        get { return _remainingTime; }
        set { _remainingTime = value; OnPropertyChanged(); }
    }
}

public class EvaluationRouteData {
    public TopicSubjectDto? TopicSubject { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
}

public partial class EvaluationViewModel(
    ILogger<EvaluationViewModel> logger,
    IMapper mapper,
    SettingService settingService,
    AnswerRecordService answerRecordService,
    NotificationService notificationService,
    EvaluationRouteData evaluationRouteData) : ObservableObject, INavigationAware, IDisposable {
    private static readonly TimeSpan DefaultTickTimeSpan = TimeSpan.FromSeconds(1);
    private readonly DispatcherTimer _dispatcherTimer = new() { Interval = DefaultTickTimeSpan };
    private bool _disposedValue;

    [ObservableProperty]
    private string? _subjectName;
    [ObservableProperty]
    private DifficultyLevel _level;
    [ObservableProperty]
    private TimeSpan _duration;
    [ObservableProperty]
    private ObservableCollection<AnswerRecordDtoNotifyChanged> _answerRecords = [];
    [ObservableProperty]
    private AnswerRecordDto? _selectedAnswerRecord;
    [ObservableProperty]
    private bool _isExpanded;
    public ExamParameter ExamParameter { get; } = new();

    public void OnNavigatedFrom() {
        _dispatcherTimer.Stop();
        _dispatcherTimer.Tick -= OnDispatcherTimer_Tick;
        SelectedAnswerRecord = null;
        AnswerRecords.Clear();
    }

    public async void OnNavigatedTo() {
        SubjectName = evaluationRouteData.TopicSubject?.Name ?? string.Empty;
        Level = evaluationRouteData.DifficultyLevel;
        await LoadAnswerRecordsCommand.ExecuteAsync(null);
        _dispatcherTimer.Tick += OnDispatcherTimer_Tick;
        await OnLoadSettingAsync();
    }

    private bool _isRuningTick;
    private void OnDispatcherTimer_Tick(object? sender, EventArgs e) {
        if (_isRuningTick) return;
        _isRuningTick = true;
        foreach (var item in AnswerRecords) {
            if (item.RemainingTime > TimeSpan.Zero) {
                item.RemainingTime -= DefaultTickTimeSpan;
                if (item.RemainingTime < TimeSpan.Zero) {
                    item.RemainingTime = TimeSpan.Zero;
                }
            }
        }
        var items = AnswerRecords.Where(v => v.RemainingTime <= TimeSpan.Zero).ToArray();
        foreach (var item in items) {
            AnswerRecords.Remove(item);
        }
        if (AnswerRecords.Count == 0) {
            _dispatcherTimer.Stop();
            IsExpanded = false;
        }
        _isRuningTick = false;
    }

    [RelayCommand]
    private async Task OnRouteToAnswerPageAsync() {
        if (evaluationRouteData.TopicSubject is null) return;

        var create = mapper.Map<EvaluationRecordCreate>(ExamParameter);
        create = create with {
            TopicSubjectId = evaluationRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = evaluationRouteData.DifficultyLevel,
            DurationSeconds = (int)Duration.TotalSeconds,
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
        routeData.IsShowSwitchAnalysisButton = false;
        routeData.IsAutomaticNextTopic = true;
        routeData.AnswerRecord = serviceResult.Result;

        App.GetRequiredService<AnswerWindow>().FullScree();
    }

    [RelayCommand]
    private async Task OnLoadAnswerRecordsAsync() {
        if (evaluationRouteData.TopicSubject is null) return;

        var answerRecordService = App.GetRequiredService<AnswerRecordService>();
        var filter = new AnswerRecordFilter {
            TopicSubjectId = evaluationRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = evaluationRouteData.DifficultyLevel,
            IsSubmission = false,
            ShouldContinueAnswering = true,
            AnswerRecordType = AnswerRecordType.Evaluation,
        };
        var result = await answerRecordService.GetListAsync(filter, Paging.None);
        _dispatcherTimer.Stop();
        if (result.IsSuccess is false) {
            AnswerRecords.Clear();
            IsExpanded = false;
            return;
        }

        var recordItems = mapper.Map<AnswerRecordDtoNotifyChanged[]>(result.Result.Items);
        IsExpanded = recordItems.Length > 0;
        if (IsExpanded) {
            AnswerRecords = new(recordItems);
            _dispatcherTimer.Start();
        }
    }

    [RelayCommand]
    private async Task OnRouteToAnswerPageUseRecordAsync() {
        if (SelectedAnswerRecord is null) {
            if (AnswerRecords.Count == 0) {
                return;
            }
            SelectedAnswerRecord = AnswerRecords[0];
        }

        var result = await answerRecordService.GetEntityByIdAsync(SelectedAnswerRecord.AnswerRecordId);
        if (result.IsSuccess is false) {
            return;
        }

        if (result.Result.StartTime < DateTime.UtcNow.AddSeconds(-result.Result.DurationSeconds + 10)) {
            return;
        }

        var routeData = App.GetRequiredService<AnswerRouteData>();
        routeData.IsShowSubmissionButton = true;
        routeData.IsShowSwitchAnalysisButton = false;
        routeData.IsAutomaticNextTopic = true;
        routeData.AnswerRecord = result.Result;

        App.GetRequiredService<AnswerWindow>().FullScree();
    }

    [RelayCommand]
    private async Task OnCleanAnswerRecordAsync() {
        if (evaluationRouteData.TopicSubject is null) return;

        var answerRecordService = App.GetRequiredService<AnswerRecordService>();
        var filter = new AnswerRecordFilter {
            TopicSubjectId = evaluationRouteData.TopicSubject.TopicSubjectId,
            DifficultyLevel = evaluationRouteData.DifficultyLevel,
            IsSubmission = false,
            AnswerRecordType = AnswerRecordType.Evaluation,
        };
        await answerRecordService.CleanAsync(filter);
        await OnLoadAnswerRecordsAsync();
    }

    private async Task OnLoadSettingAsync() {
        var json = await settingService.GetOrAddStringValueAsync(SettingConstants.ExamParameter, () => JsonSerializer.Serialize(ExamParameter));
        if (string.IsNullOrWhiteSpace(json) is false) {
            try {
                var result = JsonSerializer.Deserialize<ExamParameter>(json);
                mapper.Map(result, ExamParameter);
            }
            catch (JsonException ex) {
                logger.LogError(ex, "反序列化 Setting {name} {json} 失败", SettingConstants.ExamParameter, json);
            }
        }
        Duration = TimeSpan.FromSeconds(ExamParameter.DurationSecond);
        OnPropertyChanged(nameof(ExamParameter));
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposedValue) {
            if (disposing) {
                _dispatcherTimer.IsEnabled = false;
                _dispatcherTimer.Stop();
            }

            _disposedValue = true;
        }
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
