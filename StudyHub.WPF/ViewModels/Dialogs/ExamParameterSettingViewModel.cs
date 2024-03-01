using System.Text.Json;

using Mapster;

using MapsterMapper;

using StudyHub.Service;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Helpers;

namespace StudyHub.WPF.ViewModels.Dialogs;

internal class ExamParameterSettingViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<ExamParameter, ExamParameterSettingViewModel>()
            .ConstructUsing(() => ExceptionHelper.ThrowCannotCreateEntityException<ExamParameterSettingViewModel>())
            .Map(dest => dest.TotalMinutes, src => src.DurationSecond / 60);

        config.NewConfig<ExamParameterSettingViewModel, ExamParameter>()
            .Map(dest => dest.DurationSecond, src => src.TotalMinutes * 60);
    }
}

public class ExamParameter {
    public int SingleCount { get; set; } = 10;
    public int SingleScore { get; set; } = 1;
    public int MultipleCount { get; set; } = 10;
    public int MultipleScore { get; set; } = 2;
    public int TrueFalseCount { get; set; } = 10;
    public int TrueFalseScore { get; set; } = 2;
    public int FillCount { get; set; } = 10;
    public int FillScore { get; set; } = 5;
    public int DurationSecond { get; set; } = 3600;
    public int TotalScore => SingleCount * SingleScore +
        MultipleCount * MultipleScore +
        TrueFalseCount * TrueFalseScore +
        FillCount * FillScore;
}

public partial class ExamParameterSettingViewModel(SettingService settingService, IMapper mapper) : ObservableObject {
    public static IReadOnlyList<int> Scores { get; } = [1, 2, 3, 5];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _singleCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _multipleCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _trueFalseCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _fillCount;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _singleScore;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _multipleScore;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _trueFalseScore;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    private int _fillScore;
    [ObservableProperty]
    private int _totalMinutes;

    public int TotalScore =>
        SingleCount * SingleScore +
        MultipleCount * MultipleScore +
        TrueFalseCount * TrueFalseScore +
        FillCount * FillScore;

    [RelayCommand]
    private async Task OnLoadAsync() {
        var param = new ExamParameter();
        var result = await settingService.GetEntityByTokenAsync(SettingConstants.ExamParameter);
        if (result.IsSuccess) {
            try {
                var p = JsonSerializer.Deserialize<ExamParameter>(result.Result.SettingValue);
                param = p ?? param;
            }
            catch (JsonException) {
                await settingService.DeleteByTokenAsync(SettingConstants.ExamParameter);
            }
        }
        mapper.Map(param, this);
    }

    [RelayCommand]
    private async Task OnSaveAsync() {
        var data = mapper.Map<ExamParameter>(this);
        var json = JsonSerializer.Serialize(data);
        await settingService.CreateOrUpdateAsync(SettingConstants.ExamParameter, json);
    }
}
