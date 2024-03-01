using MapsterMapper;

using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Services;
using StudyHub.WPF.ViewModels.Dialogs;
using StudyHub.WPF.Views.Dialogs;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

public enum AssessmentType {
    None,
    /// <summary>
    /// 记录浏览
    /// </summary>
    Explorer,
    /// <summary>
    /// 测评
    /// </summary>
    Evaluation,
    /// <summary>
    /// 模拟
    /// </summary>
    Simulation,
    /// <summary>
    /// 练习
    /// </summary>
    Practice,
    /// <summary>
    /// 错题重做
    /// </summary>
    Redo,
    /// <summary>
    /// 考试记录
    /// </summary>
    Record,
}

public class AssessmentViewModelRouteData {
    /// <summary>
    /// 需要刷新科目
    /// </summary>
    public bool NeedToRefresh { get; set; }
}

public partial class AssessmentViewModel(
    IMapper mapper,
    INavigationService navigationService,
    NotificationService notificationService,
    AssessmentViewModelRouteData assessmentViewModelRouteData,
    SelectTopicSubjectDialog selectCourseSubjectDialog,
    SelectTopicSubjectViewModel selectTopicSubjectViewModel) : ObservableObject, INavigationAware {

    private bool _isInitialized = false;

    public async void OnNavigatedTo() {
        if (assessmentViewModelRouteData.NeedToRefresh is true || _isInitialized is false) {
            await LoadSucjectCommand.ExecuteAsync(null);
            assessmentViewModelRouteData.NeedToRefresh = false;
        }
        await LoadSummaryInfoCommand.ExecuteAsync(null);
    }

    public void OnNavigatedFrom() {
    }

    [ObservableProperty]
    private TopicSubjectDto? _topicSubject;
    [ObservableProperty]
    private DifficultyLevel _difficultyLevel = DifficultyLevel.Easy;
    [ObservableProperty]
    private AssessmentSummaryInformationDto? _summaryInfo;

    [RelayCommand]
    private async Task OnLoadSucjectAsync() {
        await selectTopicSubjectViewModel.LoadCommand.ExecuteAsync(null);
        DifficultyLevel = (DifficultyLevel)selectTopicSubjectViewModel.SelectedLevel;
        TopicSubject = selectTopicSubjectViewModel.SelectedSubject;
        _isInitialized = TopicSubject is not null;
    }

    [RelayCommand]
    private async Task OnLoadSummaryInfoAsync() {
        if (TopicSubject is not null) {
            var assessmentSummaryInformationService = App.GetRequiredService<AssessmentSummaryInformationService>();
            var result = await assessmentSummaryInformationService.GetSummaryInformationAsync(TopicSubject.TopicSubjectId, DifficultyLevel);
            if (result.IsSuccess) {
                SummaryInfo = result.Result;
                return;
            }
        }
        SummaryInfo = null;
    }

    [RelayCommand]
    private void OnRouteTo(AssessmentType assessmentType) {
        if (TopicSubject is null) {
            notificationService.ShowInfo("没有选择科目");
            return;
        }

        if (assessmentType is AssessmentType.Evaluation) {
            var routeData = App.GetRequiredService<EvaluationRouteData>();
            mapper.Map(this, routeData);
            navigationService.NavigateWithHierarchy(typeof(EvaluationPage));
        }
        else if (assessmentType is AssessmentType.Record) {
            var routeData = App.GetRequiredService<RecordRouteData>();
            mapper.Map(this, routeData);
            navigationService.NavigateWithHierarchy(typeof(RecordPage));
        }
        else if (assessmentType is AssessmentType.Redo) {
            var routeData = App.GetRequiredService<RedoRouteData>();
            mapper.Map(this, routeData);
            navigationService.NavigateWithHierarchy(typeof(RedoPage));
        }
        else if (assessmentType is AssessmentType.Simulation) {
            var routeData = App.GetRequiredService<SimulationRouteData>();
            mapper.Map(this, routeData);
            navigationService.NavigateWithHierarchy(typeof(SimulationPage));
        }
        else if (assessmentType is AssessmentType.Practice) {
            var routeData = App.GetRequiredService<PracticeRouteData>();
            mapper.Map(this, routeData);
            navigationService.NavigateWithHierarchy(typeof(PracticePage));
        }
    }

    [RelayCommand]
    private async Task OnSelectCourseSubject() {
        var result = await selectCourseSubjectDialog.ShowAsync();
        if (result is ContentDialogResult.Primary) {
            DifficultyLevel = (DifficultyLevel)selectTopicSubjectViewModel.SelectedLevel;
            TopicSubject = selectTopicSubjectViewModel.SelectedSubject;
            await OnLoadSummaryInfoAsync();
        }
    }
}
