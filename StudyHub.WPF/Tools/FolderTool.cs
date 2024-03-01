using System.IO;
using System.Text;

namespace StudyHub.WPF.Tools;

public sealed class FolderTool {
    /// <summary>
    /// 获取指定目录下的一个新的文件夹名称
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    public static string GetNewDirectoryName(string rootDirectory, string directoryName) {
        if (Directory.Exists(Path.Combine(rootDirectory, directoryName)) is false) {
            return directoryName;
        }
        int i = 0;
        var stringBuilder = new StringBuilder();
        do {
            i++;
            stringBuilder.Append(directoryName);
            stringBuilder.Append('-');
            stringBuilder.Append(i);
            var name = stringBuilder.ToString();
            stringBuilder.Clear();
            if (Directory.Exists(Path.Combine(rootDirectory, name)) is false) {
                return name;
            }
        } while (true);
    }

    /// <summary>
    /// 获取指定目录下的一个新的文件名称
    /// </summary>
    /// <param name="rootDirectory">目录</param>
    /// <param name="fileName">文件名，不能包含子路径</param>
    /// <returns></returns>
    public static string GetNewFileName(string rootDirectory, string fileName) {
        if (Path.Exists(Path.Combine(rootDirectory, fileName)) is false) {
            return fileName;
        }
        var ext = Path.GetExtension(fileName);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        int i = 0;
        var stringBuilder = new StringBuilder();
        do {
            i++;
            stringBuilder.Append(fileNameWithoutExt);
            stringBuilder.Append('-');
            stringBuilder.Append(i);
            stringBuilder.Append(ext);
            var name = stringBuilder.ToString();
            stringBuilder.Clear();
            if (File.Exists(Path.Combine(rootDirectory, name)) is false) {
                return name;
            }
        } while (true);
    }

    public static bool IsPathInFolder(string pathToCheck, string targetFolderPath) {
        // 获取绝对路径
        string fullPathToCheck = Path.GetFullPath(pathToCheck);
        string fullTargetFolderPath = Path.GetFullPath(targetFolderPath);

        // 使用字符串比较或其他方式判断路径是否在目标文件夹下
        return fullPathToCheck.StartsWith(fullTargetFolderPath, StringComparison.OrdinalIgnoreCase);
    }
}
