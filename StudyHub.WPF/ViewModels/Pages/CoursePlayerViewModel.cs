using FlyleafLib.MediaPlayer;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using StudyHub.Service;
using StudyHub.Service.Models;
using StudyHub.WPF.Helpers;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Pages;

internal class CoursePlayerViewModelRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<CoursePlayerRouteData, CoursePlayerViewModel>()
            .ConstructUsing(() => ExceptionHelper.ThrowCannotCreateEntityException<CoursePlayerViewModel>())
            .Map(dest => dest.Sections, src => src.Course!.CourseSections, should => should.Course != null);
    }
}

public class CoursePlayerRouteData {
    public int? SelectedSectionId { get; set; }
    public CourseDto? Course { get; set; }
}

/// <summary>
/// 必须在CoursePlayerPage实例化之后再使用
/// </summary>
/// <param name="coursePlayerRouteData"></param>
/// <param name="mapper"></param>
public partial class CoursePlayerViewModel(CoursePlayerRouteData coursePlayerRouteData, IMapper mapper) : ObservableObject, INavigationAware {
    public Player Player { get; private set; } = null!;

    public void SetPlayer(Player player) {
        player.Audio.Mute = true;
        Player = player;
        player.PlaybackStopped += Player_PlaybackStopped;
    }

    private void Player_PlaybackStopped(object? sender, PlaybackStoppedArgs e) {
        if (e.Success is false) return;
        if (sender is not Player player) return;
        if (player.Status is Status.Ended) {
            // 自动切集需要在Player_PlaybackStopped方法调用之后才能执行，否则会死循环
            Task.Delay(200).ContinueWith(v => NextCommand.Execute(null));
        }
    }

    public void OnNavigatedFrom() {
        Player.Stop();
        ResetState();
    }

    public void OnNavigatedTo() {
        mapper.Map(coursePlayerRouteData, this);
        if (Sections.Length > 0) {
            if (coursePlayerRouteData.SelectedSectionId is null) {
                SelectedIndex = 0;
            }
            else {
                int i = 0;
                foreach (var item in Sections) {
                    if (item.CourseSectionId == coursePlayerRouteData.SelectedSectionId) {
                        SelectedIndex = i;
                        break;
                    }
                    i++;
                }
            }
        }
    }

    [ObservableProperty]
    private CourseDto? _course;
    [ObservableProperty]
    private CourseSectionDto[] _sections = [];
    [ObservableProperty]
    private int _selectedIndex = -1;

    partial void OnSelectedIndexChanged(int value) {
        if (value < Sections.Length && value >= 0) {
            var sestion = Sections[value];
            Player.Open(sestion.RelativePath);
            if (sestion.Duration == TimeSpan.Zero) {
                _ = UpdateSectionDurationAsync(sestion.CourseSectionId, TimeSpan.FromTicks(Player.Duration));
            }
        }
    }

    private void ResetState() {
        Sections = [];
        SelectedIndex = -1;
        Course = null;
    }

    [RelayCommand]
    private void OnNext() {
        if (SelectedIndex < Sections.Length - 1) {
            SelectedIndex++;
        }
    }

    [RelayCommand]
    private void OnPrevious() {
        if (SelectedIndex > 0) {
            SelectedIndex--;
        }
    }

    private static async Task UpdateSectionDurationAsync(int sectionId, TimeSpan duration) {
        using var scope = App.CreateAsyncScope();
        var courseService = scope.ServiceProvider.GetRequiredService<CourseService>();
        await courseService.UpdateDurationAsync(sectionId, duration);
    }
}
