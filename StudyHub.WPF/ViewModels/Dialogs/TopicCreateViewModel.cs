using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Helpers;
using StudyHub.WPF.UserControls;

namespace StudyHub.WPF.ViewModels.Dialogs;

internal class TopicCreateViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<int, TopicCardSingleOption>()
            .Map(dest => dest.Code, src => (char)(src + 64));

        config.NewConfig<int, TopicCardMultipleOption>()
            .Map(dest => dest.Code, src => (char)(src + 64));

        config.NewConfig<int, TopicCardTrueFalseOption>()
            .Map(dest => dest.Flag, src => src == 1);

        config.NewConfig<int, TopicCardFillOption>();

        config.NewConfig<TopicCardSingleOption, TopicCardMultipleOption>()
            .Ignore(dest => dest.Code)
            .Map(dest => dest.Text, src => src.Text)
            .Map(dest => dest.IsChecked, src => src.IsChecked)
            .Map(dest => dest.IsReadOnly, src => src.IsReadOnly);

        config.NewConfig<TopicCardSingleOption, TopicOptionCreate>();
        config.NewConfig<TopicCardMultipleOption, TopicOptionCreate>();

        config.NewConfig<TopicCreateViewModel, TopicCreate>()
            .Map(dest => dest.DifficultyLevel, src => src.Level)
            .AfterMapping((src, dest) => {
                var mapper = App.GetRequiredService<IMapper>();
                if (src.TopicType == (int)TopicType.Single) {
                    dest.TopicOptions = mapper.Map<TopicOptionCreate[]>(src.TopicOptions.Cast<TopicCardSingleOption>());
                }
                else if (src.TopicType == (int)TopicType.Multiple) {
                    dest.TopicOptions = mapper.Map<TopicOptionCreate[]>(src.TopicOptions.Cast<TopicCardMultipleOption>());
                }
            });

        config.NewConfig<TopicCreateViewModel, TopicUpdate>()
            .Map(dest => dest.DifficultyLevel, src => src.Level)
            .AfterMapping((src, dest) => {

            });

        config.NewConfig<TopicDto, TopicCreateViewModel>()
            .ConstructUsing(() => ExceptionHelper.ThrowCannotCreateEntityException<TopicCreateViewModel>())
            .Map(dest => dest.Level, src => src.DifficultyLevel);

        config.NewConfig<TopicDto, AnswerRecordItemDto>()
            .Map(dest => dest.AnswerText, src => src.CorrectAnswer);
    }
}

public partial class TopicCreateViewModel(IMapper mapper, TopicSubjectOptionService topicSubjectOptionService) : ObservableValidator {
    public static IReadOnlyList<LabelValueOption> LevelOptions => EnumerationOptionService.GetDifficultyLevelOptions();
    public static IReadOnlyList<LabelValueOption> TopicTypeOptions => EnumerationOptionService.GetTopicTypeOptions();
    public TopicBankFlag TopicBankFlag { get; set; }
    public int TopicId { get; set; }
    public bool IsError { get; private set; }
    public bool IsCreate => TopicId == 0;
    public string DialogTitle => IsCreate ? "创建题目" : "编辑题目";

    [ObservableProperty]
    private int _topicType;
    [ObservableProperty]
    private LabelValueOption[] _subjectOptions = [];
    [ObservableProperty]
    private int _topicSubjectId;
    [ObservableProperty]
    private object[] _topicOptions = [];
    [ObservableProperty]
    private string _topicText = string.Empty;
    [ObservableProperty]
    private int _level = 1;
    [ObservableProperty]
    private string _analysis = string.Empty;

    partial void OnTopicTypeChanged(int value) {
        if (IsCreate) {
            TopicOptions = CreateTopicOptions((TopicType)TopicType);
        }
    }

    private object[] CreateTopicOptions(TopicType topicType) {
        object[] newOptions = topicType switch {
            Storage.Entities.TopicType.Single => mapper.Map<TopicCardSingleOption[]>(Enumerable.Range(1, 4)),
            Storage.Entities.TopicType.Multiple => mapper.Map<TopicCardMultipleOption[]>(Enumerable.Range(1, 4)),
            Storage.Entities.TopicType.TrueFalse => mapper.Map<TopicCardTrueFalseOption[]>(Enumerable.Range(1, 2)),
            Storage.Entities.TopicType.Fill => mapper.Map<TopicCardFillOption[]>(Enumerable.Range(1, 1)),
            _ => throw new NotImplementedException("不支持的题目类型"),
        };
        return newOptions;
    }

    [RelayCommand]
    private void OnCreateTopicOptions() {
        TopicOptions = CreateTopicOptions((TopicType)TopicType);
    }

    [RelayCommand]
    private async Task OnLoadAsync() {
        OnPropertyChanged(nameof(IsCreate));
        OnPropertyChanged(nameof(DialogTitle));
        await OnLoadSubjectOptionsAsync();
        await OnLoadTopicAsync();
    }

    [RelayCommand]
    private async Task OnLoadSubjectOptionsAsync() {
        SubjectOptions = await topicSubjectOptionService.GetOptionsAsync();
        if (SubjectOptions.Length > 0)
            TopicSubjectId = SubjectOptions[0].Id;
    }

    [RelayCommand]
    private async Task OnLoadTopicAsync() {
        if (TopicId == 0) {
            TopicType = (int)Storage.Entities.TopicType.Single;
        }
        else {
            var topicService = App.GetRequiredService<TopicService>();
            var result = await topicService.GetEntityByIdAsync(TopicId);
            if (result.IsSuccess is false) {
                return;
            }
            mapper.Map(result.Result, this);
            TopicOptions = mapper.Map<TopicCardModel>(mapper.Map<AnswerRecordItemDto>(result.Result)).Options;
        }
    }

    [RelayCommand]
    private async Task OnSaveAsync() {
        using var scope = App.CreateAsyncScope();
        var topicService = scope.ServiceProvider.GetRequiredService<TopicService>();
        var submissionItem = mapper.Map<SubmissionItemModel>(new TopicCardModel { Options = TopicOptions, TopicType = (TopicType)TopicType });
        if (TopicId == 0) {
            var create = mapper.Map<TopicCreate>(this);
            create.CorrectAnswer = submissionItem.AnswerText ?? string.Empty;
            var result = await topicService.CreateAsync(create);
            if (result.IsSuccess is false) {
                return;
            }
        }
        else {
            var update = mapper.Map<TopicUpdate>(this);
            update.CorrectAnswer = submissionItem.AnswerText ?? string.Empty;
            var result = await topicService.UpdateAsync(TopicId, update);
            if (result.IsSuccess is false) {
                return;
            }
        }
        await OnLoadAsync();
    }
}
