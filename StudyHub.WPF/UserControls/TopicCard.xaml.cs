using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using StudyHub.Common;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Tools.Extension;

namespace StudyHub.WPF.UserControls;

public class TopicCardModel {
    public int AnswerRecordItemId { get; set; }
    public TopicType TopicType { get; set; }
    public string LabelTopicType => TopicType.GetDescription();
    public int Order { get; set; }
    public string TopicText { get; set; } = string.Empty;
    public object[] Options { get; set; } = [];

    private string _answerText = string.Empty;
    public string AnswerText {
        set => _answerText = value;
        get => FormatAnswer(_answerText);
    }

    private string _curentAnswer = string.Empty;
    public string CorrectAnswer {
        set => _curentAnswer = value;
        get => FormatAnswer(_curentAnswer);
    }

    public string Analysis { get; set; } = string.Empty;

    private string FormatAnswer(string answer) {
        if (string.IsNullOrWhiteSpace(answer)) return answer;
        if (TopicType is TopicType.TrueFalse) {
            return answer == "1" ? "对" : "错";
        }
        return answer;
    }
}

public class PropertyWithValueChangedEventArgs(string? propertyName, object? oldValue, object? newValue) : PropertyChangedEventArgs(propertyName) {
    public object? OldValue { get; } = oldValue;
    public object? NewValue { get; } = newValue;
}

public interface INotifyPropertyWithValueChanged : INotifyPropertyChanged {
    public event EventHandler<PropertyWithValueChangedEventArgs>? PropertyWithValueChanged;
}

public abstract class ModelNotifyPropertyChanged : INotifyPropertyWithValueChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<PropertyWithValueChangedEventArgs>? PropertyWithValueChanged;

    protected virtual void OnPropertyChanged(object? oldValue, object? newValue, [CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        PropertyWithValueChanged?.Invoke(this, new PropertyWithValueChangedEventArgs(propertyName, oldValue, newValue));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TopicCardSingleOption : ModelNotifyPropertyChanged {
    public char Code { get; set; }
    private bool _isChecked;
    public bool IsChecked {
        get { return _isChecked; }
        set {
            var oldValue = _isChecked;
            _isChecked = value;
            OnPropertyChanged(oldValue, value);
        }
    }
    public string Text { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
}

public class TopicCardMultipleOption : ModelNotifyPropertyChanged {
    public char Code { get; set; }
    private bool _isChecked;
    public bool IsChecked {
        get { return _isChecked; }
        set {
            var oldValue = _isChecked;
            _isChecked = value;
            OnPropertyChanged(oldValue, value);
        }
    }
    public string Text { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
}

public class TopicCardTrueFalseOption : ModelNotifyPropertyChanged {
    public bool Flag { get; set; }
    private bool _isChecked;
    public bool IsChecked {
        get { return _isChecked; }
        set {
            var oldValue = _isChecked;
            _isChecked = value;
            OnPropertyChanged(oldValue, value);
        }
    }
    public bool IsReadOnly { get; set; }

    public static TopicCardTrueFalseOption TrueValue => new() {
        Flag = true,
    };

    public static TopicCardTrueFalseOption FalseValue => new() {
        Flag = false,
    };
}

public class TopicCardFillOption : ModelNotifyPropertyChanged {
    private string _answer = string.Empty;
    public string Answer {
        get { return _answer; }
        set {
            var oldValue = _answer;
            _answer = value;
            OnPropertyChanged(oldValue, value);
        }
    }
    public bool IsReadOnly { get; set; }
}

public partial class TopicCard : ICommandSource {
    public static readonly string RadioGroupName = "PART_RadioGroupName_TopicCard";

    public TopicCard() {
        InitializeComponent();
        LayoutUpdated += OnLayoutUpdated;
    }

    private void OnLayoutUpdated(object? sender, EventArgs e) {
        GiveTextBoxFocus();
    }

    public TopicCardModel Model {
        get { return (TopicCardModel)GetValue(ModelProperty); }
        set { SetValue(ModelProperty, value); }
    }

    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register("Model", typeof(TopicCardModel), typeof(TopicCard), new PropertyMetadata(default(TopicCardModel), new PropertyChangedCallback(OnModelChanged)));

    public bool IsOpenAnalysis {
        get { return (bool)GetValue(IsOpenAnalysisProperty); }
        set { SetValue(IsOpenAnalysisProperty, value); }
    }

    public static readonly DependencyProperty IsOpenAnalysisProperty =
        DependencyProperty.Register("IsOpenAnalysis", typeof(bool), typeof(TopicCard), new PropertyMetadata(false));

    public object CommandParameter {
        get { return GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register("CommandParameter", typeof(object), typeof(TopicCard));

    public IInputElement CommandTarget {
        get { return (IInputElement)GetValue(CommandTargetProperty); }
        set { SetValue(CommandTargetProperty, value); }
    }

    public static readonly DependencyProperty CommandTargetProperty =
        DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(TopicCard));

    public ICommand Command {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(TopicCard), new PropertyMetadata(default(ICommand), new PropertyChangedCallback(OnCommandChanged)));

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var tc = (TopicCard)d;
        if (e.OldValue is TopicCardModel oldValue) {
            tc.UnBindingModelNotifyPropertyChanged(oldValue);
        }
        if (e.NewValue is TopicCardModel newValue) {
            tc.BindingModelNotifyPropertyChanged(newValue);
            if (newValue.TopicType is TopicType.Fill) {
                tc._needFocus = true;
            }
        }
    }

    private bool _needFocus;
    private void GiveTextBoxFocus() {
        if (_needFocus is false) return;
        _needFocus = false;
        var box = this.FindDescendant<Wpf.Ui.Controls.TextBox>();
        if (box != null && box.IsReadOnly is false) {
            box.Focus();
        }
    }

    private void BindingModelNotifyPropertyChanged(TopicCardModel model) {
        foreach (var item in model.Options) {
            if (item is ModelNotifyPropertyChanged notify) {
                notify.PropertyWithValueChanged += OnValueChanged;
            }
        }
    }

    private void UnBindingModelNotifyPropertyChanged(TopicCardModel model) {
        foreach (var item in model.Options) {
            if (item is ModelNotifyPropertyChanged notify) {
                notify.PropertyWithValueChanged -= OnValueChanged;
            }
        }
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var tc = (TopicCard)d;
        tc.HookUpCommand((ICommand)e.OldValue, (ICommand)e.NewValue);
    }

    private void HookUpCommand(ICommand oldCommand, ICommand newCommand) {
        if (oldCommand != null) {
            RemoveCommand(oldCommand);
        }
        AddCommand(newCommand);
    }

    private void RemoveCommand(ICommand oldCommand) {
        EventHandler handler = CanExecuteChanged;
        oldCommand.CanExecuteChanged -= handler;
    }

    private void AddCommand(ICommand newCommand) {
        EventHandler handler = new(CanExecuteChanged);
        _canExecuteChangedHandler = handler;
        if (newCommand != null) {
            newCommand.CanExecuteChanged += _canExecuteChangedHandler;
        }
    }

    protected EventHandler? _canExecuteChangedHandler;

    private void CanExecuteChanged(object? sender, EventArgs e) {
        if (Command != null) {
            IsEnabled = Command is RoutedCommand command ? command.CanExecute(CommandParameter, CommandTarget) : Command.CanExecute(CommandParameter);
        }
    }

    private static bool IsPositiveChange(object? sender, PropertyWithValueChangedEventArgs e) {
        if (sender is TopicCardSingleOption or TopicCardMultipleOption or TopicCardTrueFalseOption) {
            return e.NewValue is true;
        }
        else if (sender is TopicCardFillOption) {
            return true;
        }
        return false;
    }

    protected void OnValueChanged(object? sender, PropertyWithValueChangedEventArgs e) {
        if (Command != null && IsPositiveChange(sender, e)) {
            if (Command is RoutedCommand command) {
                command.Execute(CommandParameter, CommandTarget);
            }
            else {
                Command.Execute(CommandParameter);
            }
        }
    }
}
