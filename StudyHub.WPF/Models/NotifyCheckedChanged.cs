using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudyHub.WPF.Models;

public partial class SupportsSelectAllOfDataContext<T> : INotifyPropertyChanged, INotifyPropertyChanging where T : notnull {
    public event PropertyChangedEventHandler? PropertyChanged;

    public event PropertyChangingEventHandler? PropertyChanging;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null) {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    private bool? _selectAll = false;
    public bool? SelectAll {
        get { return _selectAll; }
        set {
            OnPropertyChanging();
            _selectAll = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<NotifyCheckedChanged<T>> _items = [];
    public ObservableCollection<NotifyCheckedChanged<T>> Items {
        get { return _items; }
        set {
            OnPropertyChanging();
            OnItemsChanging(_items, value);
            _items = value;
            OnItemsChanged(value);
            OnPropertyChanged();
        }
    }

    private NotifyCheckedChanged<T>? _selectedItem;
    public NotifyCheckedChanged<T>? SelectedItem {
        get { return _selectedItem; }
        set {
            OnPropertyChanging();
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    private int _selectedCount;
    private bool _isFromOnSelectAllChecked;

    private void OnItemsChanging(ObservableCollection<NotifyCheckedChanged<T>>? oldValue, ObservableCollection<NotifyCheckedChanged<T>> newValue) {
        if (oldValue != null) {
            oldValue.CollectionChanged -= ItemsCollectionChanged;
            foreach (var item in oldValue) {
                item.PropertyChanged -= OnSingleChecked;
            }
        }
        if (newValue != null) {
            newValue.CollectionChanged += ItemsCollectionChanged; ;
            foreach (var item in newValue) {
                item.PropertyChanged += OnSingleChecked;
            }
        }
    }

    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.OldItems is not null) {
            foreach (var item in e.OldItems) {
                if (item is NotifyCheckedChanged<T> model) {
                    model.PropertyChanged -= OnSingleChecked;
                    if (model.IsChecked) _selectedCount--;
                }
            }
        }
        if (e.NewItems is not null) {
            foreach (var item in e.NewItems) {
                if (item is NotifyCheckedChanged<T> model) {
                    model.PropertyChanged += OnSingleChecked;
                }
            }
        }
    }

    private void OnItemsChanged(ObservableCollection<NotifyCheckedChanged<T>> _) {
        SelectAll = false;
        _selectedCount = 0;
    }

    [RelayCommand]
    private void OnSelectAllChecked() {
        var selected = SelectAll ??= false;
        _isFromOnSelectAllChecked = true;
        foreach (var item in Items) {
            item.IsChecked = selected;
        }
        _isFromOnSelectAllChecked = false;
        _selectedCount = selected ? Items.Count : 0;
    }

    private void OnSingleChecked(object? sender, PropertyChangedEventArgs e) {
        if (_isFromOnSelectAllChecked) return;
        _ = sender ?? throw new ArgumentNullException(nameof(sender));

        if (Items.Count == 0) {
            SelectAll = false;
            _selectedCount = 0;
            return;
        }

        var notify = (NotifyCheckedChanged<T>)sender;
        _ = notify.IsChecked ? _selectedCount++ : _selectedCount--;
        SelectAll = _selectedCount == Items.Count ? true : _selectedCount == 0 ? false : null;
    }

    public IEnumerable<NotifyCheckedChanged<T>> GetCheckedItems() {
        foreach (var item in Items) {
            if (item.IsChecked) yield return item;
        }
    }
}

public class NotifyCheckedChanged<T> : INotifyPropertyChanged where T : notnull {
    public NotifyCheckedChanged() {
        Model = default!;
    }

    public NotifyCheckedChanged(T model) {
        Model = model;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool _checked;
    public bool IsChecked {
        get { return _checked; }
        set { _checked = value; OnPropertyChanged(); }
    }

    public T Model { get; set; }
}
