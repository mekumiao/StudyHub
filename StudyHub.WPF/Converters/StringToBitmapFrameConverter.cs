using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace StudyHub.WPF.Converters;

public class StringToBitmapFrameConverter : IValueConverter {
    private readonly static BitmapFrame DefaultBitmapFrame = BitmapFrame.Create(new Uri("pack://application:,,,/StudyHub.WPF;component/Resources/Images/pencils.png"));
    private readonly static ConcurrentDictionary<string, BitmapFrame> BitmapImagesCache = [];
    public const int MaxBitmapFrameFileSize = 2 * 1024 * 1024;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is string path && string.IsNullOrEmpty(path) is false
            ? BitmapImagesCache.GetOrAdd(path, (path) => {
                var uri = new Uri(path, UriKind.Absolute);
                return uri.IsFile ? CreateBitmapFrameFromFile(path) : CreateBitmapFrameFromWebUri(uri);
            })
            : DefaultBitmapFrame;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    private static BitmapFrame CreateBitmapFrameFromWebUri(Uri uri) {
        try {
            return BitmapFrame.Create(uri);
        }
        catch (FileFormatException) {
            return DefaultBitmapFrame;
        }
    }

    private static BitmapFrame CreateBitmapFrameFromFile(string path) {
        if (File.Exists(path) is false) return DefaultBitmapFrame;
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        if (fileStream.Length > MaxBitmapFrameFileSize) return DefaultBitmapFrame;
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        try {
            return BitmapFrame.Create(memoryStream);
        }
        catch (FileFormatException) {
            return DefaultBitmapFrame;
        }
    }
}
