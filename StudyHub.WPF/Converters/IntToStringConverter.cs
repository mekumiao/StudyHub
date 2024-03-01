using System.Globalization;
using System.Windows.Data;

namespace StudyHub.WPF.Converters;

public sealed class IntToStringConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return value is string str && targetType == typeof(int) && string.IsNullOrWhiteSpace(str) is false && int.TryParse(str, out var result)
            ? (object)result
            : throw new ArgumentException("格式错误", nameof(value));
    }
}
