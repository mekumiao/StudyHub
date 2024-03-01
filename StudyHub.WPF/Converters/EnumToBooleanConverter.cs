using System.Globalization;
using System.Windows.Data;

namespace StudyHub.WPF.Converters;

public class EnumToBooleanConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (parameter is not string enumString) {
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
        }

        if (!Enum.IsDefined(typeof(Wpf.Ui.Appearance.ApplicationTheme), value)) {
            throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
        }

        var enumValue = Enum.Parse(typeof(Wpf.Ui.Appearance.ApplicationTheme), enumString);

        return enumValue.Equals(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return parameter is not string enumString
            ? throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName")
            : Enum.Parse(typeof(Wpf.Ui.Appearance.ApplicationTheme), enumString);
    }
}
