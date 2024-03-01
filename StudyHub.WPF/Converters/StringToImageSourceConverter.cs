using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace StudyHub.WPF.Converters;

[Obsolete("请使用 StringToBitmapFrameConverter")]
public class StringToImageSourceConverter : IValueConverter {
    private readonly static BitmapImage DefaultBitmapImage = new(new Uri("pack://application:,,,/StudyHub.WPF;component/Resources/Images/pencils.png"));
    private readonly static ConcurrentDictionary<string, BitmapImage> BitmapImagesCache = [];
    public const int MaxBitmapImageFileSize = 2 * 1024 * 1024;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is string path && string.IsNullOrEmpty(path) is false
            ? BitmapImagesCache.GetOrAdd(path, (path) => {
                var uri = new Uri(path, UriKind.Absolute);
                return uri.IsFile ? GetOrAddBitmapImageFromFile(path) : new BitmapImage(uri);
            })
            : (object)DefaultBitmapImage;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    private static BitmapImage GetOrAddBitmapImageFromFile(string path) {
        if (File.Exists(path) is false) return DefaultBitmapImage;
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        if (fileStream.Length > MaxBitmapImageFileSize) return DefaultBitmapImage;
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        var image = new BitmapImage();
        image.BeginInit();
        image.StreamSource = memoryStream;
        image.EndInit();
        return image;
    }
}
