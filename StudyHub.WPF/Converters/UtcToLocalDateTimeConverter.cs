using System.Globalization;
using System.Windows.Data;

namespace StudyHub.WPF.Converters;

public class UtcToLocalDateTimeConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is DateTime time ? time.ToLocalTime() : value;
        //return DateTime.SpecifyKind(DateTime.Parse(value.ToString()), DateTimeKind.Utc).ToLocalTime();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
