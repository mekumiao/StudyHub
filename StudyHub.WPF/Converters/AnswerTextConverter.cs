using System.Globalization;
using System.Windows.Data;

namespace StudyHub.WPF.Converters;

internal class AnswerTextConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is string text) {
            // 选择题
            if (targetType == typeof(bool) && parameter is char code) {
                return string.IsNullOrWhiteSpace(text) is false && (text.Length == 1 ? text[0] == code : text.Contains(code));
            }
            // 判断题
            else if (targetType == typeof(bool) && parameter is bool p) {
                return !string.IsNullOrWhiteSpace(text) && (p ? text == "1" : text == "0");
            }
            // 填空题
            else if (targetType == typeof(string)) {
                return text;
            }
        }
        throw new ArgumentException("参数格式错误", nameof(value));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
