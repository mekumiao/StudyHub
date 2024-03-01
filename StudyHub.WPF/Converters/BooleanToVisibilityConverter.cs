using System.Globalization;
using System.Windows.Data;

namespace StudyHub.WPF.Converters;

public class BooleanToVisibilityConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool b && targetType == typeof(Visibility) ? b ? Visibility.Visible : Visibility.Collapsed : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is Visibility.Visible;
    }
}
