using System.Collections;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using StudyHub.WPF.Helpers;

namespace StudyHub.WPF.ControlExtend;

#pragma warning disable CA2208 // 正确实例化参数异常
public static class DragDropRowBehavior {
    private static DataGrid? s_dataGrid;

    private static Popup? s_popup;

    private static bool s_enable;

    public static object? DraggedItem { get; set; }

    public static Popup GetPopupControl(DependencyObject obj) {
        return (Popup)obj.GetValue(PopupControlProperty);
    }

    public static void SetPopupControl(DependencyObject obj, Popup value) {
        obj.SetValue(PopupControlProperty, value);
    }

    public static readonly DependencyProperty PopupControlProperty =
        DependencyProperty.RegisterAttached("PopupControl", typeof(Popup), typeof(DragDropRowBehavior), new UIPropertyMetadata(null, OnPopupControlChanged));

    private static void OnPopupControlChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs e) {
        if (e.NewValue == null || e.NewValue is not Popup) {
            throw new ArgumentException("Popup Control should be set", "PopupControl");
        }

        s_popup = e.NewValue as Popup;

        s_dataGrid = depObject as DataGrid;
        // Check if DataGrid
        if (s_dataGrid == null)
            return;


        if (s_enable && s_popup != null) {
            s_dataGrid.BeginningEdit += new EventHandler<DataGridBeginningEditEventArgs>(OnBeginEdit);
            s_dataGrid.CellEditEnding += new EventHandler<DataGridCellEditEndingEventArgs>(OnEndEdit);
            s_dataGrid.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
            s_dataGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            s_dataGrid.MouseMove += new MouseEventHandler(OnMouseMove);
        }
        else {
            s_dataGrid.BeginningEdit -= new EventHandler<DataGridBeginningEditEventArgs>(OnBeginEdit);
            s_dataGrid.CellEditEnding -= new EventHandler<DataGridCellEditEndingEventArgs>(OnEndEdit);
            s_dataGrid.MouseLeftButtonUp -= new MouseButtonEventHandler(OnMouseLeftButtonUp);
            s_dataGrid.MouseLeftButtonDown -= new MouseButtonEventHandler(OnMouseLeftButtonDown);
            s_dataGrid.MouseMove -= new MouseEventHandler(OnMouseMove);

            s_dataGrid = null;
            s_popup = null;
            DraggedItem = null;
            IsEditing = false;
            IsDragging = false;
        }
    }

    public static bool GetEnabled(DependencyObject obj) {
        return (bool)obj.GetValue(EnabledProperty);
    }

    public static void SetEnabled(DependencyObject obj, bool value) {
        obj.SetValue(EnabledProperty, value);
    }

    // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty EnabledProperty =
        DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(DragDropRowBehavior), new UIPropertyMetadata(false, OnEnabledChanged));

    private static void OnEnabledChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs e) {
        //Check if value is a Boolean Type
        if (e.NewValue is bool == false)
            throw new ArgumentException("Value should be of bool type", "Enabled");

        s_enable = (bool)e.NewValue;
    }

    public static bool IsEditing { get; set; }

    public static bool IsDragging { get; set; }

    private static void OnBeginEdit(object? sender, DataGridBeginningEditEventArgs e) {
        IsEditing = true;
        //in case we are in the middle of a drag/drop operation, cancel it...
        if (IsDragging) ResetDragDrop();
    }

    private static void OnEndEdit(object? sender, DataGridCellEditEndingEventArgs e) {
        IsEditing = false;
    }

    /// <summary>
    /// Initiates a drag action if the grid is not in edit mode.
    /// </summary>
    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (IsEditing) return;

        var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(s_dataGrid));
        if (row == null || row.IsEditing) return;

        //set flag that indicates we're capturing mouse movements
        IsDragging = true;
        DraggedItem = row.Item;
    }

    /// <summary>
    /// Completes a drag/drop operation.
    /// </summary>
    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        if (!IsDragging || IsEditing) {
            return;
        }
        s_dataGrid!.Cursor = Cursors.Arrow;

        //get the target item
        var targetItem = s_dataGrid.SelectedItem;

        if (targetItem == null || !ReferenceEquals(DraggedItem, targetItem)) {
            //get target index
            var targetIndex = ((s_dataGrid).ItemsSource as IList)!.IndexOf(targetItem);
            if (targetIndex < 0) return;
            //remove the source from the list
            ((s_dataGrid).ItemsSource as IList)!.Remove(DraggedItem);

            //move source at the target's location
            ((s_dataGrid).ItemsSource as IList)!.Insert(targetIndex, DraggedItem);

            //select the dropped item
            s_dataGrid.SelectedItem = DraggedItem;
        }

        //reset
        ResetDragDrop();
    }

    /// <summary>
    /// Closes the popup and resets the
    /// grid to read-enabled mode.
    /// </summary>
    private static void ResetDragDrop() {
        IsDragging = false;
        s_popup!.IsOpen = false;
        s_dataGrid!.IsReadOnly = false;
    }

    /// <summary>
    /// Updates the popup's position in case of a drag/drop operation.
    /// </summary>
    private static void OnMouseMove(object sender, MouseEventArgs e) {
        if (!IsDragging || e.LeftButton != MouseButtonState.Pressed) return;
        if (s_dataGrid!.Cursor != Cursors.SizeAll) s_dataGrid.Cursor = Cursors.SizeAll;
        s_popup!.DataContext = DraggedItem;
        //display the popup if it hasn't been opened yet
        if (!s_popup.IsOpen) {
            //switch to read-only mode
            s_dataGrid.IsReadOnly = true;

            //make sure the popup is visible
            s_popup.IsOpen = true;
        }

        var popupSize = new Size(s_popup.ActualWidth, s_popup.ActualHeight);
        s_popup.PlacementRectangle = new Rect(e.GetPosition(s_dataGrid), popupSize);

        //make sure the row under the grid is being selected
        Point position = e.GetPosition(s_dataGrid);
        var row = UIHelpers.TryFindFromPoint<DataGridRow>(s_dataGrid, position);
        if (row != null) s_dataGrid.SelectedItem = row.Item;
    }
}
