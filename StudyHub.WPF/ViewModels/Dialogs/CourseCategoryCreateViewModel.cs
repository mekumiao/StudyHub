using System.ComponentModel.DataAnnotations;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Models;

namespace StudyHub.WPF.ViewModels.Dialogs;

public partial class CourseCategoryCreateViewModel(IMapper mapper) : ObservableValidator {
    public string DialogTitle => CourseCategoryId == 0 ? "创建分类" : "编辑分类";
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

    public int CourseCategoryId { get; set; }

    [RelayCommand]
    private async Task OnSaveAsync() {
        ValidateAllProperties();
        if (HasErrors) return;
        using var scope = App.CreateAsyncScope();
        var courseCategoryService = scope.ServiceProvider.GetRequiredService<CourseCategoryService>();
        if (CourseCategoryId == 0) {
            var input = mapper.Map<CourseCategoryCreate>(this);
            var result = await courseCategoryService.CreateAsync(input);
            IsError = result.IsSuccess is false;
            Message = result.Message;
        }
        else {
            var input = mapper.Map<CourseCategoryUpdate>(this);
            var result = await courseCategoryService.UpdateAsync(CourseCategoryId, input);
            IsError = result.IsSuccess is false;
            Message = result.Message;
        }
    }
}
