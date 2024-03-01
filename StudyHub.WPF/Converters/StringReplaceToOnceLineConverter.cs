using System.Globalization;
using System.Windows.Data;

namespace StudyHub.WPF.Converters;

public class StringReplaceToOnceLineConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return ((string)value).ReplaceLineEndings(" ");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
