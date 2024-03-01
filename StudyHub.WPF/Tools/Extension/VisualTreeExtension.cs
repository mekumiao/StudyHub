using System.Windows.Media;

namespace StudyHub.WPF.Tools.Extension;

public static class VisualTreeExtension {
    public static IEnumerable<T> FindAscendants<T>(this DependencyObject obj) where T : DependencyObject {
        var parent = VisualTreeHelper.GetParent(obj);
        if (parent is T target) {
            yield return target;
            foreach (var item in FindAscendants<T>(parent)) {
                yield return item;
            }
        }
        else if (parent is not null) {
            foreach (var item in FindAscendants<T>(parent)) {
                yield return item;
            }
        }
    }

    public static T? FindAscendant<T>(this DependencyObject obj) where T : DependencyObject {
        return FindAscendants<T>(obj).FirstOrDefault();
    }

    public static IEnumerable<T> FindDescendants<T>(this DependencyObject obj) where T : DependencyObject {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is T target) {
                yield return target;
                foreach (var item in FindDescendants<T>(child)) {
                    yield return item;
                }
            }
            else if (child is not null) {
                foreach (var item in FindDescendants<T>(child)) {
                    yield return item;
                }
            }
        }
    }

    public static T? FindDescendant<T>(this DependencyObject obj) where T : DependencyObject {
        return FindDescendants<T>(obj).FirstOrDefault();
    }

    public static T? FindDescendant<T>(this DependencyObject obj, Func<T, bool> predicate) where T : DependencyObject {
        return FindDescendants<T>(obj).FirstOrDefault(predicate);
    }
}
