using System.Collections.ObjectModel;

using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Service.Models;

namespace StudyHub.WPF.ViewModels.Dialogs;

public partial class SelectTopicSubjectViewModel(TopicSubjectService topicSubjectService) : ObservableObject {
    public static IReadOnlyList<LabelValueOption> LevelOptions => EnumerationOptionService.GetDifficultyLevelOptions();
    private int _selectedSubjectId;

    [ObservableProperty]
    private ObservableCollection<TopicSubjectDto> _subjects = [];
    [ObservableProperty]
    private TopicSubjectDto? _selectedSubject;
    [ObservableProperty]
    private int _selectedLevel = 1;

    partial void OnSelectedSubjectChanged(TopicSubjectDto? value) {
        if (value is not null) {
            _selectedSubjectId = value.TopicSubjectId;
        }
    }

    public bool SetSelectedSubjectBySubjectId(int subjectId) {
        if (subjectId <= 0) return false;

        SelectedSubject = Subjects.FirstOrDefault(v => v.TopicSubjectId == subjectId);
        return SelectedSubject is not null;
    }

    [RelayCommand]
    private async Task OnLoadAsync() {
        var filter = new TopicSubjectFilter { };
        var result = await topicSubjectService.GetListAsync(filter, Paging.None);
        if (result.IsSuccess is false) {
            return;
        }

        Subjects = new ObservableCollection<TopicSubjectDto>(result.Result.Items);
        if (Subjects.Count == 0) {
            SelectedSubject = default;
            _selectedSubjectId = default;
        }
        else if (SetSelectedSubjectBySubjectId(_selectedSubjectId) is false) {
            SelectedSubject = Subjects[0];
            _selectedSubjectId = SelectedSubject.TopicSubjectId;
        }
    }
}
