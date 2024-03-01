using System.ComponentModel;
using System.Windows.Input;

using StudyHub.WPF.ViewModels.Pages;
using StudyHub.WPF.Views.Pages;

namespace StudyHub.WPF.Views.Windows;

public partial class AnswerWindow {
    public AnswerWindow() {
#if !DEBUG
        // 慎用这个属性（如果此窗口发生异常，将导致用户电脑无法正常使用，特别特别影响用户体验）。没有强制答题需求的话，不要取消注释。
        //Topmost = true;
#endif
        InitializeComponent();
    }

    private bool _allowClose;
    private AnswerViewModel? _viewModel;

    public void FullScree() {
        var page = App.GetRequiredService<AnswerPage>();
        _viewModel = page.ViewModel;
        page.ViewModel.Submission += OnCloseAnswerWindow;
        try {
            page.ViewModel.OnNavigatedTo();
            mainFrame.Content = page;
            SetFullScreeStyle();
            ShowDialog();
        }
        finally {
            page.ViewModel.OnNavigatedFrom();
            _viewModel = null;
        }
    }

    /// <summary>
    /// 交卷后关闭答题窗口
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCloseAnswerWindow(object? sender, EventArgs e) {
        _allowClose = true;
        Close();
    }

    public void SetFullScreeStyle() {
        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Maximized;
    }

    protected override void OnClosing(CancelEventArgs e) {
        e.Cancel = _allowClose is false;
    }

    private void FluentWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.Escape) {
            // 交卷
            _viewModel?.SubmissionCommand.ExecuteAsync(null);
        }
    }
}
