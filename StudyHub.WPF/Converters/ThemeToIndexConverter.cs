using System.Globalization;
using System.Windows.Data;

using Wpf.Ui.Appearance;

namespace StudyHub.WPF.Converters;

public sealed class ThemeToIndexConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is ApplicationTheme.Dark ? 1 : value is ApplicationTheme.HighContrast ? 2 : (object)0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is 1 ? ApplicationTheme.Dark : value is 2 ? ApplicationTheme.HighContrast : (object)ApplicationTheme.Light;
    }
}
