using System.IO;
using System.IO.Enumeration;
using System.Reflection;

namespace StudyHub.WPF.Tools;

public class ManifestResourceTool {
    private readonly static Assembly Assembly = Assembly.GetExecutingAssembly();

    public static Stream? GetManifestResourceStream(string resourceName) {
        return Assembly.GetManifestResourceStream(resourceName);
    }

    public static string? ReadAllText(string resourceName) {
        using var stream = GetManifestResourceStream(resourceName);
        if (stream is not null) {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        return null;
    }

    public static IEnumerable<Stream> FindManifestResourceStreams(string searchPattern) {
        foreach (var name in Assembly.GetManifestResourceNames()) {
            if (FileSystemName.MatchesSimpleExpression(searchPattern.AsSpan(), name.AsSpan(), true)) {
                var stream = GetManifestResourceStream(name);
                if (stream is not null) yield return stream;
            }
        }
    }
}
