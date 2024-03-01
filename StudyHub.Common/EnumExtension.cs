using System.ComponentModel;
using System.Reflection;

namespace StudyHub.Common;

public static class EnumExtension {
    public static string GetDescription(this Enum value) {
        var strValue = value.ToString();
        var field = value.GetType().GetField(strValue);
        if (field != null) {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute) {
                return attribute.Description;
            }
        }
        return strValue;
    }

    public static IEnumerable<string> GetAllDescription<T>() where T : Enum {
        var enumType = typeof(T);
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields) {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute) {
                yield return attribute.Description;
            }
        }
    }
}
