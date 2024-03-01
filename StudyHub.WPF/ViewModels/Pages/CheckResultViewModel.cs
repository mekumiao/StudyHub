using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public class CheckResultRouteData {
    public int AnswerRecordId { get; set; }
}

public partial class CheckResultViewModel(
    CheckResultRouteData checkResultRouteData,
    INavigationService navigationService) : ObservableObject, INavigationAware {

    public void OnNavigatedFrom() {
    }

    public async void OnNavigatedTo() {
        await LoadAnswerRecordCommand.ExecuteAsync(null);
    }

    public string Title => AnswerRecord?.AnswerRecordType switch {
        AnswerRecordType.Evaluation => "考核成绩",
        AnswerRecordType.Simulation => "模拟成绩",
        AnswerRecordType.Practice => "练习结果",
        AnswerRecordType.Redo => "重做结果",
        _ => "检查结果",
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PercentScore), nameof(Title))]
    private AnswerRecordDto? _answerRecord;
    [ObservableProperty]
    private int _totalSingleCorrect;
    [ObservableProperty]
    private int _totalMultipleCorrect;
    [ObservableProperty]
    private int _totalTrueFalseCorrect;
    [ObservableProperty]
    private int _totalFillCorrect;
    /// <summary>
    /// 分数百分比值。0-100
    /// </summary>
    public double PercentScore => AnswerRecord is null ? 0 : AnswerRecord.TotalScore == 0 ? 0 : AnswerRecord.Score / (double)AnswerRecord.TotalScore * 100;

    [RelayCommand]
    private void OnRouteToDashboardPage() {
        navigationService.Navigate(typeof(AssessmentPage));
    }

    [RelayCommand]
    private async Task OnLoadAnswerRecordAsync() {
        var answerRecordService = App.GetRequiredService<AnswerRecordService>();
        var result = await answerRecordService.GetEntityByIdAsync(checkResultRouteData.AnswerRecordId);
        if (result.IsSuccess is false) {
            return;
        }
        AnswerRecord = result.Result;
        RefreshTotalCorrect();
    }

    private void RefreshTotalCorrect() {
        if (AnswerRecord is null) return;
        TotalSingleCorrect = AnswerRecord.AnswerRecordItems.Where(v => v.TopicType == TopicType.Single).Where(v => v.Status == AnswerRecordItemStatus.Correct).Count();
        TotalMultipleCorrect = AnswerRecord.AnswerRecordItems.Where(v => v.TopicType == TopicType.Multiple).Where(v => v.Status == AnswerRecordItemStatus.Correct).Count();
        TotalTrueFalseCorrect = AnswerRecord.AnswerRecordItems.Where(v => v.TopicType == TopicType.TrueFalse).Where(v => v.Status == AnswerRecordItemStatus.Correct).Count();
        TotalFillCorrect = AnswerRecord.AnswerRecordItems.Where(v => v.TopicType == TopicType.Fill).Where(v => v.Status == AnswerRecordItemStatus.Correct).Count();
    }
}
