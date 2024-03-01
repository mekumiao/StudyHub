using System.ComponentModel.DataAnnotations;
using System.IO;

namespace StudyHub.WPF.Attributes;

/// <summary>
/// 有效的路径名称
/// </summary>
public class ValidPathNameAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value is string path) {
            if (ContainsInvalidFileNameChars(path) || ContainsInvalidPathChars(path)) {
                return new("目录或文件名不能包含特殊字符");
            }
        }
        return ValidationResult.Success;
    }

    private static bool ContainsInvalidFileNameChars(string fileName) {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return fileName.IndexOfAny(invalidChars) != -1;
    }

    private static bool ContainsInvalidPathChars(string path) {
        char[] invalidChars = Path.GetInvalidPathChars();
        return path.IndexOfAny(invalidChars) != -1;
    }
}
