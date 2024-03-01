namespace StudyHub.Common;

public static class EnumerableExtension {
    public static int FindIndex<TSource>(this IEnumerable<TSource> items, Func<TSource, bool> predicate) {
        int i = 0;
        foreach (var item in items) {
            if (predicate(item)) {
                return i;
            }
            i++;
        }
        return -1;
    }
}
