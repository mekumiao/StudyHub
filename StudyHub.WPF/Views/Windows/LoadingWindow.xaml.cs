using System.ComponentModel;

namespace StudyHub.WPF.Views.Windows;

[INotifyPropertyChanged]
public partial class LoadingWindow : Window {
    public LoadingWindow() {
        DataContext = this;
        InitializeComponent();

        _ = StartProgressAsync();
    }

    private double _progressValue;
    public double ProgressValue {
        get { return _progressValue; }
        set { _progressValue = value; OnPropertyChanged(); }
    }

    private async Task StartProgressAsync() {
        for (int i = 1; i < 100; i++) {
            int result = IncreasingFunction(i);
            ProgressValue = result / 100;
            await Task.Delay(50);
        }
    }

    private static int IncreasingFunction(int input) {
        return (int)Math.Pow(input, 2);
    }

    protected override void OnClosing(CancelEventArgs e) {
        ProgressValue = 100;
        base.OnClosing(e);
    }
}
