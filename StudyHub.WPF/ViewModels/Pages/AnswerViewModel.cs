using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Common;
using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Controls;
using StudyHub.WPF.UserControls;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

internal class AnswerViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<TopicOptionDto, TopicCardSingleOption>();
        config.NewConfig<TopicOptionDto, TopicCardMultipleOption>();
        config.NewConfig<AnswerRecordItemDto, TopicCardModel>()
            .Ignore(dest => dest.Options)
            .Map(dest => dest.Analysis, src => src.Topic.Analysis, should => should.Topic != null)
            .AfterMapping((src, dest) => {
                var mapper = App.GetRequiredService<IMapper>();
                if (src.TopicType is TopicType.Single) {
                    var items = mapper.Map<TopicCardSingleOption[]>(src.TopicOptions);
                    dest.Options = items;
                    if (string.IsNullOrWhiteSpace(src.AnswerText) is false) {
                        foreach (var item in items) {
                            item.IsReadOnly = true;
                            if (item.Code == src.AnswerText[0]) {
                                item.IsChecked = true;
                            }
                        }
                    }
                }
                else if (src.TopicType is TopicType.Multiple) {
                    var items = mapper.Map<TopicCardMultipleOption[]>(src.TopicOptions);
                    dest.Options = items;
                    if (string.IsNullOrWhiteSpace(src.AnswerText) is false) {
                        foreach (var item in items) {
                            if (src.AnswerText.Contains(item.Code)) {
                                item.IsChecked = true;
                            }
                            item.IsReadOnly = true;
                        }
                    }
                }
                else if (src.TopicType is TopicType.TrueFalse) {
                    TopicCardTrueFalseOption[] items = [TopicCardTrueFalseOption.TrueValue, TopicCardTrueFalseOption.FalseValue];
                    dest.Options = items;
                    if (string.IsNullOrWhiteSpace(src.AnswerText) is false) {
                        foreach (var item in items) {
                            item.IsChecked = src.AnswerText[0] == '1' == item.Flag;
                            item.IsReadOnly = true;
                        }
                    }
                }
                else if (src.TopicType is TopicType.Fill) {
                    TopicCardFillOption[] items = [new TopicCardFillOption { Answer = src.AnswerText ?? string.Empty }];
                    dest.Options = items;
                    if (string.IsNullOrWhiteSpace(src.AnswerText) is false) {
                        foreach (var item in items) {
                            item.IsReadOnly = true;
                        }
                    }
                }
            });

        config.NewConfig<AnswerRecordItemDto, TopicSheetItem>()
            .IgnoreNonMapped(true)
            .Map(dest => dest.Order, src => src.Order)
            .Map(dest => dest.Status, src => src.Status);

        config.NewConfig<TopicCardModel, SubmissionItemModel>()
            .Ignore(dest => dest.AnswerText!)
            .AfterMapping((src, dest) => {
                if (src.TopicType is TopicType.Single) {
                    dest.AnswerText = src.Options.Cast<TopicCardSingleOption>().Where(v => v.IsChecked).Select(v => v.Code.ToString()).FirstOrDefault();
                }
                else if (src.TopicType is TopicType.Multiple) {
                    dest.AnswerText = string.Join("", src.Options.Cast<TopicCardMultipleOption>().Where(v => v.IsChecked).Select(v => v.Code));
                }
                else if (src.TopicType is TopicType.TrueFalse) {
                    dest.AnswerText = src.Options.Cast<TopicCardTrueFalseOption>().Where(v => v.IsChecked).Select(v => v.Flag ? "1" : "0").FirstOrDefault();
                }
                else if (src.TopicType is TopicType.Fill) {
                    dest.AnswerText = src.Options.Cast<TopicCardFillOption>().Select(v => v.Answer).FirstOrDefault();
                }
            });
    }
}

public class AnswerRouteData {
    /// <summary>
    /// 是否显示答案解析按钮
    /// </summary>
    public bool IsShowSwitchAnalysisButton { get; set; }
    /// <summary>
    /// 是否显示交卷按钮
    /// </summary>
    public bool IsShowSubmissionButton { get; set; }
    /// <summary>
    /// 是否开启自动下一题功能
    /// </summary>
    public bool IsAutomaticNextTopic { get; set; }
    /// <summary>
    /// 浏览模式下仅显示错题
    /// </summary>
    public bool OnlyDisplayIncorrectInBrowse { get; set; }
    /// <summary>
    /// 题目数据
    /// </summary>
    public AnswerRecordDto? AnswerRecord { get; set; }
}

public partial class AnswerViewModel(
    INavigationService navigationService,
    IMapper mapper,
    AnswerRouteData answerRouteData) : ObservableObject, INavigationAware {
    private readonly INavigationView _navigationView = navigationService.GetNavigationControl();
    public event EventHandler? Submitting;
    public event EventHandler? Submission;

    public virtual void OnNavigatedTo() {
        _navigationView.Navigating += OnNavigating;
        mapper.Map(answerRouteData, this);
        Board = answerRouteData.AnswerRecord;
        OnPropertyChanged(nameof(SheetMode));
    }

    public virtual void OnNavigatedFrom() {
        _navigationView.Navigating -= OnNavigating;
        answerRouteData.IsShowSubmissionButton = false;
        answerRouteData.IsShowSwitchAnalysisButton = false;
        answerRouteData.IsAutomaticNextTopic = false;
        //answerRouteData.AnswerRecord = null; // 不做清空，回退到此页时可查看题目
        ResetTopicCommand.Execute(this);
    }

    protected void OnSubmitting() {
        Submitting?.Invoke(this, EventArgs.Empty);
    }

    protected void OnSubmission() {
        Submission?.Invoke(this, EventArgs.Empty);
    }

    private async void OnNavigating(NavigationView sender, NavigatingCancelEventArgs args) {
        if (args.Page is not CheckResultPage && RemainingTime > TimeSpan.Zero && IsShowSubmissionButton) {
            args.Cancel = !await ShowConfirmBackwardsNavigated();
        }
    }

    private AnswerRecordDto? _board;

    public AnswerRecordDto? Board {
        set {
            _board = value;
            if (value?.AnswerRecordItems.Count() > 0) {
                var source = value.AnswerRecordItems.OrderBy(v => v.TopicType);
                _topicCardModels = mapper.Map<TopicCardModel[]>(source);
                OnCurrentIndexChanged(0);

                var sheetHeaders = source.GroupBy(v => v.TopicType).Select(v => new TopicSheetHeader {
                    Title = v.Key.GetDescription(),
                    Children = mapper.Map<TopicSheetItem[]>(v),
                }).ToArray();

                TopicSheets = new TopicSheetDataContext(sheetHeaders);
                _sawtoothArrayAccessor = new SawtoothArrayAccessor<TopicSheetItem>(sheetHeaders.Select(v => v.Children).ToArray());

                if (value.StartTime.HasValue) {
                    RemainingTime = value.StartTime.Value.AddSeconds(value.DurationSeconds) - DateTime.UtcNow;
                }

                if (IsShowSubmissionButton && value.AnswerRecordType is AnswerRecordType.Evaluation or AnswerRecordType.Simulation) {
                    IsEnableCountdownTimer = true;
                }
            };
        }
        get => _board;
    }

    [ObservableProperty]
    private TopicCardModel? _currentCardModel;
    [ObservableProperty]
    private int _currentIndex;
    private TopicCardModel[] _topicCardModels = [];

    [ObservableProperty]
    private bool _isOpenAnalysis;
    [ObservableProperty]
    private TopicType _topicType;
    [ObservableProperty]
    private bool _isShowSwitchAnalysisButton;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SheetMode))]
    private bool _isShowSubmissionButton;
    [ObservableProperty]
    private TopicSheetDataContext _topicSheets = [];
    private SawtoothArrayAccessor<TopicSheetItem>? _sawtoothArrayAccessor;
    [ObservableProperty]
    private TimeSpan _remainingTime;
    [ObservableProperty]
    private bool _isEnableCountdownTimer;

    public TopicSheetMode SheetMode => IsShowSubmissionButton ? TopicSheetMode.Answer : answerRouteData.OnlyDisplayIncorrectInBrowse ? TopicSheetMode.IncorrectlyBrowse : TopicSheetMode.Browse;

    partial void OnCurrentIndexChanged(int value) {
        if (value >= 0 && value < _topicCardModels.Length) {
            CurrentCardModel = _topicCardModels[value];
        }
    }

    [RelayCommand]
    private async Task OnSubmissionAsync(bool? isCountdownEnd) {
        if (Board is null) {
            return;
        }

        var model = new SubmissionModel {
            AnswerRecordId = Board.AnswerRecordId,
            SubmissionItems = mapper.Map<SubmissionItemModel[]>(_topicCardModels),
        };

        if (isCountdownEnd is not true) {
            if (model.SubmissionItems.Any(v => string.IsNullOrWhiteSpace(v.AnswerText))
                && await ShowConfirmSubmissionNavigated() is false) {
                return;
            }
        }

        OnSubmitting();

        using var scope = App.CreateAsyncScope();
        var answerService = scope.ServiceProvider.GetRequiredService<AnswerRecordService>();
        // 交卷
        var result = await answerService.SubmissionAsync(model);
        if (result.IsSuccess is false) {
            return;
        }
        // 批改
        var result2 = await answerService.CorrectingAnswerRecordAsync(Board.AnswerRecordId);
        if (result2.IsSuccess is false) {
            return;
        }

        OnSubmission();

        var routeData = App.GetRequiredService<CheckResultRouteData>();
        routeData.AnswerRecordId = Board.AnswerRecordId;
        navigationService.NavigateWithHierarchy(typeof(CheckResultPage));
    }

    [RelayCommand]
    private void OnPreviousTopic() {
        if (CurrentIndex > 0) CurrentIndex--;
    }

    [RelayCommand]
    private void OnSwitchAnalysis() {
        IsOpenAnalysis = !IsOpenAnalysis;
    }

    [RelayCommand]
    private void OnNextTopic() {
        if (CurrentIndex < TopicSheets.Total - 1) CurrentIndex++;
    }

    [RelayCommand]
    private void OnResetTopic() {
        _board = null;
        CurrentIndex = -1;
        CurrentCardModel = null;
        _topicCardModels = [];
        TopicSheets = [];
        IsEnableCountdownTimer = false;
        RemainingTime = TimeSpan.Zero;
        _sawtoothArrayAccessor = null;
        IsOpenAnalysis = false;
        IsShowSubmissionButton = false;
        IsShowSwitchAnalysisButton = false;
        TopicType = TopicType.None;
    }

    [RelayCommand]
    private async Task OnUserAnswersAsync() {
        //不能用交卷的情况下不触发作答行为
        if (IsShowSubmissionButton is false) return;

        if (_sawtoothArrayAccessor is not null) {
            var item = _sawtoothArrayAccessor[CurrentIndex];
            if (item != null && item.Status is TopicCellStatus.None) {
                item.Status = TopicCellStatus.Answer;
            }
        }

        // 记录用户答案
        if (CurrentCardModel is not null) {
            var item = mapper.Map<SubmissionItemModel>(CurrentCardModel);
            var answerRecordService = App.GetRequiredService<AnswerRecordService>();
            await answerRecordService.RecordUserAnswerAsync(item);
        }

        if (answerRouteData.IsAutomaticNextTopic) {
            // 仅单选题和判断题支持自动下一题
            if (CurrentCardModel is null or { TopicType: TopicType.Single or TopicType.TrueFalse }) {
                await Task.Delay(150).ContinueWith(task => OnNextTopic());
            }
        }
    }

    private async Task<bool> ShowConfirmBackwardsNavigated() {
        var messageBox = new MessageBox {
            Title = "提示",
            Content = "考试进行中，确认要退出吗？",
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
        };

        Submitting += CloseDialog;

        void CloseDialog(object? sender, EventArgs args) {
            messageBox?.ClickClose();
            Submitting -= CloseDialog;
        }

        var result = await messageBox.ShowDialogAsync();
        return result is MessageBoxResult.Primary;
    }

    private async Task<bool> ShowConfirmSubmissionNavigated() {
        var messageBox = new MessageBox {
            Title = "提示",
            Content = "有未答的题目，确认要交卷吗？",
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
        };

        Submitting += CloseDialog;

        void CloseDialog(object? sender, EventArgs args) {
            messageBox?.ClickClose();
            Submitting -= CloseDialog;
        }

        var result = await messageBox.ShowDialogAsync();
        return result is MessageBoxResult.Primary;
    }
}

/// <summary>
/// 锯齿数组访问器
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="values"></param>
public class SawtoothArrayAccessor<T>(T[][] values) where T : notnull {
    public T? this[int index] {
        get {
            if (values.Length == 0 || index < 0) return default;
            int end = 0;
            for (int i = 0; i < values.Length; i++) {
                int jl = values[i].Length;
                if (jl == 0) continue;
                end += jl;
                if (index >= end) continue;
                return values[i][^(end - index)];
            }
            return default;
        }
    }
}
