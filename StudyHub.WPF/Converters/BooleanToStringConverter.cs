using System.Globalization;
using System.Windows.Data;

namespace StudyHub.WPF.Converters;

internal class BooleanToStringConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool b ? b ? "对" : "错" : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
