using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StudyHub.WPF.Tools;
using StudyHub.WPF.Views.Pages;
using StudyHub.WPF.Views.Windows;

using StudyHubDb;

namespace StudyHub.WPF.Services;

public class ApplicationHostService(
    IServiceProvider serviceProvider,
    ILogger<ApplicationHostService> logger) : IHostedService {

    public async Task StartAsync(CancellationToken cancellationToken) {
        await HandleInitializeDataAsync();
        await HandleActivationAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        await Task.CompletedTask;
    }

    private async Task HandleInitializeDataAsync() {
        await SeedData.EnsureSeedDataAsync(serviceProvider);
        await SyncTopicsAndCouresAsync();
    }

    private async Task HandleActivationAsync() {
        await Task.CompletedTask;
        if (!Application.Current.Windows.OfType<MainWindow>().Any()) {
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Loaded += OnNavigationWindowLoaded;
            mainWindow.Loaded += OnCloseLoadingWindow;
            mainWindow.ContentRendered += OnActivateMainWindow;
            logger.LogDebug("显示主窗口");
            mainWindow.Show();
        }
    }

    private void OnNavigationWindowLoaded(object sender, RoutedEventArgs e) {
        if (sender is not MainWindow navigationWindow) {
            return;
        }
        navigationWindow.NavigationView.Navigate(typeof(AssessmentPage));
    }

    private void OnCloseLoadingWindow(object sender, RoutedEventArgs e) {
        App.CloseLoadingWindow();
    }

    private void OnActivateMainWindow(object? sender, EventArgs e) {
        App.ActivateMainWindow();
    }

    private async Task SyncTopicsAndCouresAsync() {
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var scope = scopeFactory.CreateAsyncScope();
        var dataSyncService = scope.ServiceProvider.GetRequiredService<DataSyncService>();
        try {
            await dataSyncService.SyncCouresOnlyOnceAsync();
            await dataSyncService.SyncTopicsOnlyOnceAsync(ManifestResourceTool.FindManifestResourceStreams);
        }
        catch (Exception ex) {
            logger.LogError(ex, "在启动期间运行初始化数据同步任务失败");
        }
    }
}
