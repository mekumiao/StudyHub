using System.Windows.Media;

using StudyHub.WPF.ViewModels.Windows;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

using WPFDevelopers.Helpers;

namespace StudyHub.WPF.Views.Windows;

public partial class MainWindow : INavigationWindow {
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        ISnackbarService snackbarService,
        IContentDialogService contentDialogService) {
        SystemThemeWatcher.Watch(this, WindowBackdropType.Auto, false);
        ApplicationThemeManager.Changed += OnThemeChanged;

        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        navigationService.SetNavigationControl(NavigationView);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        contentDialogService.SetContentPresenter(RootContentDialog);
        NavigationView.SetServiceProvider(serviceProvider);
    }

    private void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent) {
        ControlsHelper.ToggleLightAndDark(currentApplicationTheme is ApplicationTheme.Dark);
    }

    private bool _isUserClosedPane;
    private bool _isPaneOpenedOrClosedFromCode;

    private void OnNavigationViewNavigated(NavigationView sender, NavigatedEventArgs args) {
        NeedHeaderVisibility(args.Page.GetType(), [
            typeof(CourseCategoriesPage),
            typeof(CourseListPage),
            typeof(TopicSubjectListPage),
            typeof(TopicListPage),
            typeof(SimulationTopicListPage),
            typeof(RecordPage),
            typeof(RedoPage),
            typeof(SimulationPage),
            typeof(PracticePage),
            typeof(EvaluationPage),
            typeof(IncorrectlyTopicExplorerPage),
        ]);

        void NeedHeaderVisibility(Type target, Type[] extTypes) {
            NavigationView.HeaderVisibility = extTypes.Any(v => v == target) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        if (_isUserClosedPane) {
            return;
        }

        _isPaneOpenedOrClosedFromCode = true;
        NavigationView.IsPaneOpen = !(e.NewSize.Width <= 900);
        _isPaneOpenedOrClosedFromCode = false;
    }

    private void NavigationView_OnPaneOpened(NavigationView sender, RoutedEventArgs args) {
        if (_isPaneOpenedOrClosedFromCode) {
            return;
        }
        _isUserClosedPane = false;
    }

    private void NavigationView_OnPaneClosed(NavigationView sender, RoutedEventArgs args) {
        if (_isPaneOpenedOrClosedFromCode) {
            return;
        }
        _isUserClosedPane = true;
    }

    public INavigationView GetNavigation() => NavigationView;

    public bool Navigate(Type pageType) => NavigationView.Navigate(pageType);

    public void SetServiceProvider(IServiceProvider serviceProvider) {
        NavigationView.SetServiceProvider(serviceProvider);
    }

    public void SetPageService(IPageService pageService) => NavigationView.SetPageService(pageService);

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    private void OnExitMenuItemClick(object sender, RoutedEventArgs e) {
        Close();
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        Shutdown();
    }

    private static void Shutdown() {
        Application.Current.Shutdown();
    }
}
