using System.Globalization;
using System.Windows.Data;

using StudyHub.Common;

namespace StudyHub.WPF.Converters;

public class EnumToDescriptionConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is Enum v ? v.GetDescription() : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
