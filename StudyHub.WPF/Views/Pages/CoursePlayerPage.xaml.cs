using FlyleafLib.MediaPlayer;
using FlyleafLib;

using StudyHub.WPF.Helpers;
using StudyHub.WPF.ViewModels.Pages;

using Wpf.Ui.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace StudyHub.WPF.Views.Pages;

public partial class CoursePlayerPage : INavigableView<CoursePlayerViewModel>, INotifyPropertyChanged, IDisposable {
    public CoursePlayerViewModel ViewModel { get; }
    public Player Player { get; private set; }
    public Config Config { get; private set; }

    public CoursePlayerPage(
        CoursePlayerViewModel viewModel) {
        FlyleafLibHelper.EngineStart();
        ViewModel = viewModel;
        InitializeComponent();
        Config = new Config();
#if DEBUG
        Config.Player.Stats = true;
#endif
        Player = new Player(Config);
        viewModel.SetPlayer(Player);
        DataContext = this;
    }

    //public string SampleVideo { get; set; } = Utils.FindFileBelow("Sample.mp4");

    private string _lastError = string.Empty;
    public string LastError {
        get => _lastError;
        set {
            if (_lastError == value) return;
            _lastError = value; OnPropertyChanged(LastError);
        }
    }

    private bool _showDebug;
    private bool _disposedValue;

    public bool ShowDebug {
        get => _showDebug;
        set {
            if (_showDebug == value) return; _showDebug = value;
            OnPropertyChanged(nameof(ShowDebug));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ICommand? ToggleDebug { get; set; }

    protected virtual void Dispose(bool disposing) {
        if (!_disposedValue) {
            if (disposing) {
                Player.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
