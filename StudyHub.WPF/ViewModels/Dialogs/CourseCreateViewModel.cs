using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using Ookii.Dialogs.Wpf;

using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.WPF.Attributes;
using StudyHub.WPF.Models;
using StudyHub.WPF.Services;
using StudyHub.WPF.Tools;

namespace StudyHub.WPF.ViewModels.Dialogs;

internal class CourseCreateViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<CourseSectionDto, NotifyCheckedChanged<CourseSectionDto>>()
            .Map(dest => dest.Model, src => src);
    }
}

public partial class CourseCreateViewModel(
    IMapper mapper,
    ILogger<CourseCreateViewModel> logger,
    NotificationService notificationService) : ObservableValidator {
    private readonly ProgressDialog _progressDialog = new() {
        WindowTitle = "复制文件",
        Text = "正在复制文件中...",
        Description = "Processing...",
        ShowTimeRemaining = true,
    };

    public bool IsCreate => CourseId == 0;
    public string DialogTitle => IsCreate ? "创建课程" : "编辑课程";
    public event EventHandler? SavedEvent;

    protected void OnSavedEvent() {
        IsSaved = true;
        SavedEvent?.Invoke(this, EventArgs.Empty);
    }

    [ObservableProperty]
    private bool _isError;
    [ObservableProperty]
    private string? _message;
    public bool IsSaved { get; private set; }

    [ObservableProperty]
    private int _courseId;
    [ObservableProperty]
    private string _category = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required, StringLength(100, MinimumLength = 1)]
    private string _title = string.Empty;
    [ObservableProperty]
    [StringLength(500)]
    private string _description = string.Empty;
    [ObservableProperty]
    private string _cover = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required, StringLength(100, MinimumLength = 1), ValidPathName]
    private string _relativePath = string.Empty;
    [ObservableProperty]
    private LabelValueOption[] _categories = [];

    public SupportsSelectAllOfDataContext<CourseSectionDto> SectionDataContext { get; } = new();
    /// <summary>
    /// 移除的项
    /// </summary>
    private readonly List<CourseSectionDto> _removedSections = [];
    private CourseDto? _courseCache;

    [RelayCommand]
    private async Task OnLoadAsync() {
        _removedSections.Clear();
        OnPropertyChanged(nameof(IsCreate));
        OnPropertyChanged(nameof(DialogTitle));
        await OnLoadCourseCategoryOptionsAsync();
        await OnLoadCourseAsync();
    }

    private async Task OnLoadCourseCategoryOptionsAsync() {
        using var scope = App.CreateAsyncScope();
        var courseCategoryOptionService = scope.ServiceProvider.GetRequiredService<CourseCategoryOptionService>();
        Categories = await courseCategoryOptionService.GetOptionsAsync();
        if (Categories.Length > 0) {
            // 课程同步时将分类名称作为标识
            Category = Categories[0].Text;
        }
    }

    private async Task OnLoadCourseAsync() {
        if (CourseId > 0) {
            using var scope = App.CreateAsyncScope();
            var courseService = scope.ServiceProvider.GetRequiredService<CourseService>();
            var result = await courseService.GetEntityByIdAsync(CourseId);
            if (result.IsSuccess is false) {
                SectionDataContext.Items.Clear();
                return;
            }
            _courseCache = result.Result;
            mapper.Map(result.Result, this);
            var notifies = mapper.Map<NotifyCheckedChanged<CourseSectionDto>[]>(result.Result.CourseSections);
            SectionDataContext.Items = new ObservableCollection<NotifyCheckedChanged<CourseSectionDto>>(notifies);
        }
    }

    [RelayCommand]
    private void OnSave() {
        ValidateAllProperties();
        if (HasErrors) return;

        _progressDialog.DoWork += CopyFileToCourseDirectory;
        _progressDialog.RunWorkerCompleted += CopyFileToCourseDirectoryCompleted;
        _progressDialog.ShowDialog();
    }

    /// <summary>
    /// 获取或创建课程目录
    /// </summary>
    /// <remarks>如果给定的目录在<see cref="CoursesRootDirectory"/>中已经存在，则将在文件夹后添加序号，直到不存在为止</remarks>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">目录格式异常</exception>
    public static DirectoryInfo GetOrCreateDirectoryInfoByDirectoryName(string directoryName) {
        var dirName = Path.GetDirectoryName(directoryName) ?? throw new ArgumentException("课程目录不能为空", nameof(directoryName));
        if (string.IsNullOrEmpty(dirName) is false) throw new ArgumentException("课程目录不能包含子路径", nameof(directoryName));

        var courseDirectory = Path.Combine(CourseService.CoursesRootDirectory, directoryName);
        if (Directory.Exists(courseDirectory)) {
            if (Directory.GetFileSystemEntries(courseDirectory).Length == 0) {
                return new DirectoryInfo(courseDirectory);
            }
            directoryName = FolderTool.GetNewDirectoryName(courseDirectory, directoryName);
            var directoryInfo = new DirectoryInfo(directoryName);
            directoryInfo.Create();
            return directoryInfo;
        }
        else {
            var directoryInfo = new DirectoryInfo(courseDirectory);
            directoryInfo.Create();
            return directoryInfo;
        }
    }

    /// <summary>
    /// 当文件复制完成时
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CopyFileToCourseDirectoryCompleted(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
        if (e.Result is null) return;

        var input = (CourseOverwrite)e.Result;
        using var scope = App.CreateAsyncScope();
        var courseService = scope.ServiceProvider.GetRequiredService<CourseService>();
        try {
            var result = await courseService.CourseOverwriteAsync(input);
            if (result.IsSuccess is false) {
                notificationService.ShowWarning(result.Message);
                return;
            }
            OnSavedEvent();
        }
        catch (Exception ex) {
            logger.LogError(ex, "覆盖课程信息时发生错误");
            notificationService.ShowError(ex.Message);
        }
    }

    /// <summary>
    /// 复制新的课程到课程目录
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CopyFileToCourseDirectory(object? sender, System.ComponentModel.DoWorkEventArgs e) {
        _progressDialog.DoWork -= CopyFileToCourseDirectory;
        var directoriInfo = default(DirectoryInfo);
        try {
            // 编辑模式下，需要移除标记为删除的项
            if (IsCreate is false) {
                foreach (var item in _removedSections) {
                    // 移除在课程根目录下的文件
                    if (Path.IsPathRooted(item.RelativePath) && FolderTool.IsPathInFolder(item.RelativePath, CourseService.CoursesRootDirectory)) {
                        if (File.Exists(item.RelativePath)) {
                            File.Delete(item.RelativePath);
                        }
                    }
                }
            }
            if (IsCreate) {
                directoriInfo = GetOrCreateDirectoryInfoByDirectoryName(RelativePath);
            }
            else {
                directoriInfo = new DirectoryInfo(Path.Combine(CourseService.CoursesRootDirectory, RelativePath));
                if (directoriInfo.Exists is false) throw new Exception("课程目录已被删除，请同步后重试");
            }

            var result = mapper.Map<CourseOverwrite>(this);
            result.RelativePath = directoriInfo.Name;

            var goods = SectionDataContext.Items
                .Select(v => v.Model)
                .Where(v => string.IsNullOrWhiteSpace(v.RelativePath) is false)
                .ToArray();

            if (string.IsNullOrWhiteSpace(Cover) is false && (_courseCache is null || _courseCache.Cover != Cover)) {
                var newName = FolderTool.GetNewFileName(directoriInfo.FullName, Path.GetFileName(Cover));
                var destFullName = Path.Combine(directoriInfo.FullName, newName);
                result.Cover = newName;
                File.Copy(Cover, destFullName);
            }

            var createlist = new List<CourseSectionCreate>();
            int i = 0, max = goods.Length;

            foreach (var item in goods) {
                if (_progressDialog.CancellationPending) {
                    _progressDialog.ReportProgress((int)(i / (double)max * 100), "取消复制，正在移除残留文件...", "");
                    directoriInfo.Delete(true);
                    return;
                }

                var sectionCreate = mapper.Map<CourseSectionCreate>(item);
                createlist.Add(sectionCreate);

                // 仅复制新的项到课程目录
                if (item.CourseId == 0) {
                    // 确保文件名在课程目录中不存在
                    var newName = FolderTool.GetNewFileName(directoriInfo.FullName, Path.GetFileName(item.RelativePath));
                    var destFullName = Path.Combine(directoriInfo.FullName, newName);
                    File.Copy(item.RelativePath, destFullName);
                    // 修改 RelativePath 字段值
                    sectionCreate.RelativePath = newName;
                    if (max < 50) Thread.Sleep(200);
                }

                i++;
                _progressDialog.ReportProgress((int)(i / (double)max * 100), "复制文件中到课程目录...", item.RelativePath);
            }

            _progressDialog.ReportProgress(100, "复制完成", "");

            result.Sections = [.. createlist];
            e.Result = result;
        }
        catch (Exception ex) {
            IsError = true;
            Message = ex.Message;
            if (directoriInfo?.Exists is true) {
                directoriInfo.Delete(true);
            }
        }
        finally {
            _progressDialog.DoWork -= CopyFileToCourseDirectory;
        }
    }

    [RelayCommand]
    private void OnSelectCoverOnExplorer() {
        var dialog = new OpenFileDialog {
            Title = "选择课程封面图片",
            Filter = "PNG图片(*.png)|*.png",
        };
        if (dialog.ShowDialog() is true) {
            Cover = dialog.FileName;
        }
    }

    [RelayCommand]
    private void OnCleanSections() {
        // 记录删除项
        _removedSections.AddRange(SectionDataContext.Items.Where(v => v.Model.CourseId > 0).Select(v => v.Model));

        SectionDataContext.Items = [];
    }

    [RelayCommand]
    private void OnRemoveSelectedSections() {
        var items = SectionDataContext.GetCheckedItems().ToArray();
        if (items.Length == 0) {
            notificationService.ShowInfo("没有任何选中项");
            return;
        }

        // 仅记录CourseId>0的删除项
        _removedSections.AddRange(SectionDataContext.Items.Where(v => v.IsChecked && v.Model.CourseId > 0).Select(v => v.Model));

        var notifies = mapper.Map<NotifyCheckedChanged<CourseSectionDto>[]>(SectionDataContext.Items.Where(v => v.IsChecked is false));
        SectionDataContext.Items = new ObservableCollection<NotifyCheckedChanged<CourseSectionDto>>(notifies);
    }

    /// <summary>
    /// 新添加的路径转<see cref="CourseSectionDto"/>
    /// </summary>
    /// <param name="fileNames"></param>
    /// <returns></returns>
    private static IEnumerable<CourseSectionDto> FileNamesMapToCourseSectionCreate(string[] fileNames) {
        foreach (var item in fileNames) {
            var name = Path.GetFileNameWithoutExtension(item);
            yield return new CourseSectionDto {
                Title = name,
                Description = name,
                RelativePath = item,
            };
        }
    }

    [RelayCommand]
    private void OnSelectSectionsOnExplorer() {
        var dialog = new OpenFileDialog {
            Title = "选择课节视频",
            Multiselect = true,
            Filter = "视频文件 (*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.ts)|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.ts",
        };
        if (dialog.ShowDialog() is true) {
            var sections = mapper.Map<CourseSectionDto[]>(FileNamesMapToCourseSectionCreate(dialog.FileNames));
            var notifies = mapper.Map<NotifyCheckedChanged<CourseSectionDto>[]>(sections);
            foreach (var item in notifies) {
                SectionDataContext.Items.Add(item);
            }
        }
    }
}
