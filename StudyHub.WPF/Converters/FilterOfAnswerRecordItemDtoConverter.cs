using System.Globalization;
using System.Windows.Data;

using StudyHub.Service.Models;
using StudyHub.Storage.Entities;

namespace StudyHub.WPF.Converters;

internal class FilterOfAnswerRecordItemDtoConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is IEnumerable<AnswerRecordItemDto> items) {
            if (parameter is TopicType topicType) {
                return items.Where(v => v.TopicType == topicType).ToArray();
            }
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
