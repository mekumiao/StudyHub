using System.Windows.Media;

namespace StudyHub.WPF.Helpers;

public sealed class FontFamilyHelper {
    private static readonly Uri FontBaseUri = new("pack://application:,,,/StudyHub.WPF;component/Resources/Fonts/");

    public static FontFamily IconFont => new(FontBaseUri, "./#iconfont");
}
