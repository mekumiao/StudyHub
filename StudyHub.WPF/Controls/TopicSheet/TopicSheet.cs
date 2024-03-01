using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Threading;

using StudyHub.WPF.Tools.Extension;

namespace StudyHub.WPF.Controls;

public class TopicSheetItem : INotifyPropertyChanged {
    public int Index { get; set; }
    public int Order { get; set; }

    private TopicCellStatus _status;
    public TopicCellStatus Status {
        get { return _status; }
        set { _status = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TopicSheetHeader {
    public int Index { get; set; }
    public string Title { get; set; } = string.Empty;
    public TopicSheetItem[] Children { get; set; } = [];
}

public class TopicSheetDataContext : ObservableCollection<TopicSheetHeader> {
    public TopicSheetDataContext(IEnumerable<TopicSheetHeader> items) : base(items) { }

    private int? _total;
    public int Total => _total ??= Items.Sum(v => v.Children.Length);

#if !DEBUG
    public TopicSheetDataContext() : base() {
        CollectionChanged += (sender, e) => _total = null;
    }
#else
    public TopicSheetDataContext() : base(CreateList()) {
        CollectionChanged += (sender, e) => _total = null;
    }
#endif

    private static IEnumerable<TopicSheetHeader> CreateList() {
        yield return CreateTopicSheetHeader("单选题");
        yield return CreateTopicSheetHeader("多选题");
        yield return CreateTopicSheetHeader("判断题");
        yield return CreateTopicSheetHeader("填空题");

        static TopicSheetHeader CreateTopicSheetHeader(string title) {
            return new TopicSheetHeader {
                Title = title,
                Children = [
                    new TopicSheetItem { Index = 0, Order = 1, Status = TopicCellStatus.Answer, },
                    new TopicSheetItem { Index = 1, Order = 2, Status = TopicCellStatus.None, },
                    new TopicSheetItem { Index = 2, Order = 3, Status = TopicCellStatus.Correct, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                    new TopicSheetItem { Index = 3, Order = 4, Status = TopicCellStatus.Incorrectly, },
                ],
            };
        }
    }
}

public enum TopicSheetMode {
    [Description("作答模式")]
    Answer,
    [Description("浏览模式")]
    Browse,
    [Description("错题浏览模式")]
    IncorrectlyBrowse,
}

public class TopicSheet : TreeView {
    protected const string ElementTextBox = "PART_TextBox";
    protected const string TemplateElementTopicSheetItemsPresenter = "PART_TopicSheetItemsPresenter";

    protected ItemsPresenter _topicSheetItemsPresenter = null!;
    protected TreeViewItem[] _treeItems = [];
    private DispatcherTimer? _dispatcherTimer;
    /// <summary>
    /// 倒计时结束事件
    /// </summary>
    public event EventHandler? CountdownEnd;
    /// <summary>
    /// 倒计时开始事件
    /// </summary>
    public event EventHandler? CountdownStart;

    public TopicSheet() {
        Loaded += OnCacheTreeViewItems;
        Loaded += OnSelectedFirstItem;
        Loaded += OnCreateAndStartTimer;
        Unloaded += OnCleanCacheTreeViewItems;
        Unloaded += OnCleanTimer;
    }

    private void OnCacheTreeViewItems(object sender, RoutedEventArgs e) {
        if (ItemsSource is not TopicSheetDataContext context) return;

        _treeItems = _topicSheetItemsPresenter.FindDescendants<TreeViewItem>().Where(v => v.HasItems == false).ToArray();

        int index = 0;
        foreach (var item in context) {
            foreach (var child in item.Children) {
                child.Index = index++;
            }
        }
    }

    private void OnCleanCacheTreeViewItems(object sender, RoutedEventArgs e) {
        _treeItems = [];
    }

    private void OnSelectedFirstItem(object sender, RoutedEventArgs e) {
        SelectedIndex = 0;
    }

    private bool TryGetTreeViewItemByCache(int index, [NotNullWhen(true)] out TreeViewItem? item) {
        if (_treeItems.Length > 0 && index >= 0 && index < _treeItems.Length) {
            item = _treeItems[index];
            return true;
        }
        item = null;
        return false;
    }

    protected virtual void OnCountdownStart() {
        CountdownStart?.Invoke(this, new EventArgs());
    }

    protected virtual void OnCountdownEnd() {
        CountdownEnd?.Invoke(this, new EventArgs());
    }

    private void OnCreateAndStartTimer(object sender, RoutedEventArgs e) {
        OnCleanTimer(sender, e);
        _dispatcherTimer = new DispatcherTimer();
        _dispatcherTimer.Tick += DispatcherTimer_Tick;
        _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        if (IsStartTimerAtLoaded && IsEnableCountdownTimer) {
            _dispatcherTimer.Start();
            OnCountdownStart();
        }
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e) {
        if (Duration > TimeSpan.Zero) {
            Duration -= TimeSpan.FromSeconds(1);
            if (Duration <= TimeSpan.Zero) {
                Duration = TimeSpan.Zero;
                _dispatcherTimer?.Stop();
                OnCountdownEnd();
            }
        }
    }

    private void OnCleanTimer(object sender, RoutedEventArgs e) {
        _dispatcherTimer?.Stop();
        _dispatcherTimer = null;
    }

    public void StartCountdown() {
        _dispatcherTimer?.Start();
    }

    public void StopCountdown() {
        _dispatcherTimer?.Stop();
    }

    public override void OnApplyTemplate() {
        base.OnApplyTemplate();

        _topicSheetItemsPresenter = GetTemplateChild<ItemsPresenter>(TemplateElementTopicSheetItemsPresenter);
    }

    protected T GetTemplateChild<T>(string name)
        where T : DependencyObject {
        if (GetTemplateChild(name) is not T dependencyObject) {
            throw new ArgumentNullException(name);
        }
        return dependencyObject;
    }

    public int SelectedIndex {
        get { return (int)GetValue(SelectedIndexProperty); }
        set { SetValue(SelectedIndexProperty, value); }
    }

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register("SelectedIndex", typeof(int), typeof(TopicSheet), new PropertyMetadata(-1, new PropertyChangedCallback(OnSelectedIndexChanged), new CoerceValueCallback(OnSelectedIndexCoerce)));

    public TopicSheetMode SheetMode {
        get { return (TopicSheetMode)GetValue(SheetModeProperty); }
        set { SetValue(SheetModeProperty, value); }
    }

    public static readonly DependencyProperty SheetModeProperty =
        DependencyProperty.Register("SheetMode", typeof(TopicSheetMode), typeof(TopicSheet));

    public TimeSpan Duration {
        get { return (TimeSpan)GetValue(DurationProperty); }
        set { SetValue(DurationProperty, value); }
    }

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(TopicSheet));

    public bool IsStartTimerAtLoaded {
        get { return (bool)GetValue(IsStartTimerAtLoadedProperty); }
        set { SetValue(IsStartTimerAtLoadedProperty, value); }
    }

    public static readonly DependencyProperty IsStartTimerAtLoadedProperty =
        DependencyProperty.Register("IsStartTimerAtLoaded", typeof(bool), typeof(TopicSheet), new PropertyMetadata(true));

    public bool IsEnableCountdownTimer {
        get { return (bool)GetValue(IsEnableCountdownTimerProperty); }
        set { SetValue(IsEnableCountdownTimerProperty, value); }
    }

    public static readonly DependencyProperty IsEnableCountdownTimerProperty =
        DependencyProperty.Register("IsEnableCountdownTimer", typeof(bool), typeof(TopicSheet), new PropertyMetadata(true, new PropertyChangedCallback(OnIsEnableCountdownTimerChanged)));

    private static void OnIsEnableCountdownTimerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var dp = (TopicSheet)d;
        if (e.NewValue is bool isEnable && dp._dispatcherTimer is not null) {
            dp._dispatcherTimer.IsEnabled = isEnable;
        }
    }

    /// <summary>
    /// 选中TreeViewItem
    /// </summary>
    /// <param name="index"></param>
    private void SelectedTreeViewItem(int index) {
        if (_treeItems.Length == 0) return;

        if (TryGetTreeViewItemByCache(index, out var item)) {
            item.IsSelected = true;
        }
    }

    private static object OnSelectedIndexCoerce(DependencyObject d, object baseValue) {
        var dp = (TopicSheet)d;
        var index = (int)baseValue;
        if (index >= 0 && index < dp._treeItems.Length) {
            return index;
        }
        else {
            return -1;
        }
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var sheet = (TopicSheet)d;
        int index = (int)e.NewValue;
        sheet.SelectedTreeViewItem(index);
        if (index < 0) {
            sheet.SelectedIndex = -1;
        }
    }

    protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e) {
        base.OnSelectedItemChanged(e);

        if (e.NewValue is TopicSheetItem item) {
            SelectedIndex = item.Index;
        }
    }
}
