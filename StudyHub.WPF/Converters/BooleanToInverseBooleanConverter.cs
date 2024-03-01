using System.Globalization;
using System.Windows.Data;

namespace StudyHub.WPF.Converters;

public class BooleanToInverseBooleanConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool target && targetType == typeof(bool) ? !target : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return Convert(value, targetType, parameter, culture);
    }
}
