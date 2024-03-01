using System.ComponentModel;
using System.Windows.Controls;

namespace StudyHub.WPF.Controls;

public enum TopicCellStatus {
    [Description("无")]
    None,
    [Description("已答")]
    Answer,
    [Description("正确")]
    Correct,
    [Description("错误")]
    Incorrectly,
    [Description("未答")]
    NoReply,
    [Description("选中")]
    Selected,
}

public class TopicCell : ContentControl {
    public TopicCellStatus Status {
        get { return (TopicCellStatus)GetValue(StatusProperty); }
        set { SetValue(StatusProperty, value); }
    }

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register("Status", typeof(TopicCellStatus), typeof(TopicCell), new PropertyMetadata(TopicCellStatus.None));

    public bool IsSelected {
        get { return (bool)GetValue(IsSelectedProperty); }
        set { SetValue(IsSelectedProperty, value); }
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register("IsSelected", typeof(bool), typeof(TopicCell), new PropertyMetadata(false));
}
