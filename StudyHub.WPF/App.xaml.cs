using System.IO;
using System.Windows.Threading;

using EntityFramework.Exceptions.Sqlite;

using Mapster;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OfficeOpenXml;

using Serilog;
using Serilog.Events;

using StudyHub.Service;
using StudyHub.Storage.DbContexts;
using StudyHub.WPF.Services;
using StudyHub.WPF.ViewModels.Dialogs;
using StudyHub.WPF.ViewModels.Pages;
using StudyHub.WPF.ViewModels.Windows;
using StudyHub.WPF.Views.Dialogs;
using StudyHub.WPF.Views.Pages;
using StudyHub.WPF.Views.Windows;

using StudyHubDb;

using Wpf.Ui;
using Wpf.Ui.Appearance;

using WPFDevelopers.Helpers;

namespace StudyHub.WPF;

public partial class App : Application {
    public static LoadingWindow? LoadingWindow { get; private set; }

    private static IHost? s_host;
#if !DEBUG
    private const string MutexName = "a0d68bbb-cea7-417b-8407-ebceda451654";
    private Mutex? _mutex;
#endif

    private static void CreateLogger() {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .WriteTo.File("Logs/app_.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private static IHost CreatesAppHost() {
        CreateLogger();

        return Host
          .CreateDefaultBuilder()
          .ConfigureAppConfiguration(c => c.SetBasePath(Directory.GetCurrentDirectory()))
          .ConfigureServices((context, services) => {
              TypeAdapterConfig.GlobalSettings.Default
                  .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
                  .MapToConstructor(true)
                  .IgnoreNullValues(true);
              TypeAdapterConfig.GlobalSettings.Scan(typeof(App).Assembly, typeof(TopicService).Assembly);
              TypeAdapterConfig.GlobalSettings.Compile();

              services.AddHostedService<ApplicationHostService>();
#if !DEBUG
              services.AddHostedService<NamedPipeManagerService>();
#endif
              services.AddDbContext<StudyHubDbContext>(options => {
                  options.UseSqlite("Data Source=studyhub.db", dbOpts => {
                      dbOpts.MigrationsAssembly(typeof(SeedData).Assembly.FullName);
                      dbOpts.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                  });
                  options.UseExceptionProcessor();
              });

              #region Services 注入
              services.AddMapster();
              services.AddScoped<TopicService>();
              services.AddScoped<AnswerRecordService>();
              services.AddScoped<TopicSubjectService>();
              services.AddScoped<CourseService>();
              services.AddScoped<CourseCategoryService>();
              services.AddScoped<AssessmentSummaryInformationService>();
              services.AddSingleton<IAnswerVerificationService, AnswerVerificationService>();
              services.AddSingleton<NotificationService>();
              services.AddScoped<SettingService>();
              services.AddScoped<TopicSubjectOptionService>();
              services.AddScoped<DataSyncService>();
              services.AddScoped<CourseCategoryOptionService>();
              #endregion

              #region Window 注入
              services.AddSingleton<MainWindow>();
              services.AddSingleton<MainWindowViewModel>();
              services.AddSingleton<INavigationWindow, MainWindow>();
              services.AddTransient<AnswerWindow>();
              #endregion

              #region wpf-ui
              services.AddSingleton<INavigationService, NavigationService>();
              services.AddSingleton<ISnackbarService, SnackbarService>();
              services.AddSingleton<IContentDialogService, ContentDialogService>();
              services.AddSingleton<IThemeService, ThemeService>();
              services.AddSingleton<ITaskBarService, TaskBarService>();
              #endregion

              #region RouteData 注入
              services.AddSingleton<EvaluationRouteData>();
              services.AddSingleton<AnswerRouteData>();
              services.AddSingleton<CheckResultRouteData>();
              services.AddSingleton<RecordRouteData>();
              services.AddSingleton<RedoRouteData>();
              services.AddSingleton<SimulationRouteData>();
              services.AddSingleton<PracticeRouteData>();
              services.AddSingleton<CourseInfoRouteData>();
              services.AddSingleton<CoursePlayerRouteData>();
              services.AddSingleton<CourseViewModelRouteData>();
              services.AddSingleton<AssessmentViewModelRouteData>();
              services.AddSingleton<CourseListViewModelRouteData>();
              #endregion

              #region Dialog 注入
              services.AddScoped<SelectTopicSubjectDialog>();
              services.AddScoped<SelectTopicSubjectViewModel>();
              services.AddScoped<TopicSubjectCreateDialog>();
              services.AddScoped<TopicSubjectCreateViewModel>();
              services.AddScoped<CourseCategoryCreateDialog>();
              services.AddScoped<CourseCategoryCreateViewModel>();
              services.AddScoped<ExamParameterSettingDialog>();
              services.AddScoped<ExamParameterSettingViewModel>();
              services.AddScoped<TopicCreateDialog>();
              services.AddScoped<TopicCreateViewModel>();
              services.AddScoped<CourseCreateDialog>();
              services.AddScoped<CourseCreateViewModel>();
              #endregion

              #region Page 注入
              services.AddSingleton<SettingsPage>();
              services.AddSingleton<SettingsViewModel>();
              services.AddSingleton<CoursePage>();
              services.AddSingleton<CourseViewModel>();
              services.AddSingleton<CourseInfoPage>();
              services.AddSingleton<CourseInfoViewModel>();
              services.AddSingleton<CoursePlayerPage>();
              services.AddSingleton<CoursePlayerViewModel>();
              services.AddSingleton<AssessmentPage>();
              services.AddSingleton<AssessmentViewModel>();
              services.AddSingleton<EvaluationPage>();
              services.AddSingleton<EvaluationViewModel>();
              services.AddSingleton<SimulationPage>();
              services.AddSingleton<SimulationViewModel>();
              services.AddSingleton<PracticePage>();
              services.AddSingleton<PracticeViewModel>();
              services.AddSingleton<RedoPage>();
              services.AddSingleton<RedoViewModel>();
              services.AddSingleton<RecordPage>();
              services.AddSingleton<RecordViewModel>();
              services.AddSingleton<CheckResultPage>();
              services.AddSingleton<CheckResultViewModel>();
              services.AddSingleton<TopicSubjectListPage>();
              services.AddSingleton<TopicSubjectListViewModel>();
              services.AddSingleton<TopicListPage>();
              services.AddSingleton<TopicListViewModel>();
              services.AddSingleton<CourseCategoriesPage>();
              services.AddSingleton<CourseCategoriesViewModel>();
              services.AddSingleton<CourseListPage>();
              services.AddSingleton<CourseListViewModel>();
              services.AddSingleton<SimulationTopicListPage>();
              services.AddSingleton<SimulationTopicListViewModel>();
              services.AddSingleton<IncorrectlyTopicExplorerPage>();
              services.AddSingleton<IncorrectlyTopicExplorerViewModel>();
              services.AddSingleton<ParameterSettingPage>();
              services.AddSingleton<ParameterSettingViewModel>();
              services.AddSingleton<AnswerPage>();
              services.AddSingleton<AnswerViewModel>();
              #endregion
          })
          .UseSerilog()
          .Build();
    }

    public static IServiceScope CreateScope() {
        _ = s_host ?? throw new ArgumentNullException(nameof(s_host));
        return s_host.Services.CreateScope();
    }

    public static AsyncServiceScope CreateAsyncScope() {
        _ = s_host ?? throw new ArgumentNullException(nameof(s_host));
        return s_host.Services.CreateAsyncScope();
    }

    public static T GetRequiredService<T>() where T : class {
        _ = s_host ?? throw new ArgumentNullException(nameof(s_host));
        return s_host.Services.GetRequiredService<T>();
    }

    public static MainWindow GetMainWindowFromService() => GetRequiredService<MainWindow>();

    public static void ActivateMainWindow() {
        var mainWindow = GetRequiredService<MainWindow>();
        mainWindow.Dispatcher.Invoke(() => {
            if (mainWindow.WindowState is WindowState.Minimized) {
                mainWindow.WindowState = WindowState.Normal;
            }
            mainWindow.Topmost = true;
            mainWindow.Activate();
            mainWindow.Topmost = false;
        });
    }

    public static void CloseLoadingWindow() {
        LoadingWindow?.Dispatcher.Invoke(() => LoadingWindow.Close());
    }

    public static void ApplySystemTheme() {
        if (ApplicationThemeManager.IsAppMatchesSystem() is false) {
            ApplicationThemeManager.ApplySystemTheme();
            ControlsHelper.ToggleLightAndDark();
        }
    }

    protected async override void OnStartup(StartupEventArgs e) {
#if !DEBUG
        _mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew) {
            NamedPipeManagerService.NotifyActivateMainWindow();
            _mutex.Close();
            _mutex = null;
            Environment.Exit(0);
        }
#endif
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        RegisterEvents();

        var thread = new Thread(() => {
            LoadingWindow = new LoadingWindow();
            LoadingWindow.ShowDialog();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        ApplySystemTheme();

        base.OnStartup(e);

        s_host = CreatesAppHost();
        await s_host.StartAsync();
    }

    protected async override void OnExit(ExitEventArgs e) {
#if !DEBUG
        _mutex?.ReleaseMutex();
        _mutex?.Close();
#endif
        if (s_host is not null) {
            await s_host.StopAsync();
            s_host.Dispose();
        }
        Log.CloseAndFlush();
    }

    private void RegisterEvents() {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    /// <summary>
    /// UI线程未捕获异常处理事件（UI主线程）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
        try {
            HandleException(e.Exception);
        }
        catch (Exception ex) {
            HandleException(ex);
        }
        finally {
            e.Handled = true;
        }
    }

    /// <summary>
    /// 非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
        try {
            if (e.ExceptionObject is Exception exception) {
                HandleException(exception);
            }
        }
        catch (Exception ex) {
            HandleException(ex);
        }
        finally {
            //ignore
        }
    }

    /// <summary>
    /// Task线程内未捕获异常处理事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) {
        try {
            if (e.Exception is Exception exception) {
                HandleException(exception);
            }
        }
        catch (Exception ex) {
            HandleException(ex);
        }
        finally {
            e.SetObserved();
        }
    }

    private static void HandleException(Exception ex) {
        Log.Fatal(ex, "Unhandled exception");
#if DEBUG
        throw ex;
#endif
    }
}
