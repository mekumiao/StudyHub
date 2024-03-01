using System.ComponentModel.DataAnnotations;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Models;

namespace StudyHub.WPF.ViewModels.Dialogs;

public partial class TopicSubjectCreateViewModel(IMapper mapper) : ObservableValidator {
    public string DialogTitle => TopicSubjectId == 0 ? "创建科目" : "编辑科目";
    [ObservableProperty]
    private bool _isError;
    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(AllowEmptyStrings = false), StringLength(20, MinimumLength = 1)]
    private string _name = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [StringLength(500)]
    private string _description = string.Empty;
    public int TopicSubjectId { get; set; }

    [RelayCommand]
    private async Task OnSaveAsync() {
        ValidateAllProperties();
        if (HasErrors) return;
        using var scope = App.CreateAsyncScope();
        var topicSubjectService = scope.ServiceProvider.GetRequiredService<TopicSubjectService>();
        if (TopicSubjectId == 0) {
            var input = mapper.Map<TopicSubjectCreate>(this);
            var result = await topicSubjectService.CreateAsync(input);
            IsError = result.IsSuccess is false;
            Message = result.Message;
        }
        else {
            var input = mapper.Map<TopicSubjectUpdate>(this);
            var result = await topicSubjectService.UpdateAsync(TopicSubjectId, input);
            IsError = result.IsSuccess is false;
            Message = result.Message;
        }
    }
}
