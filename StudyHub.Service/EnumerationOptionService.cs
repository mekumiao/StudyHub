using System.Collections.Concurrent;

using StudyHub.Common;
using StudyHub.Service.Models;
using StudyHub.Storage.Entities;

namespace StudyHub.Service;

public static class EnumerationOptionService {
    private readonly static ConcurrentDictionary<string, LabelValueOption[]> LabelValueOptions = [];

    private static IEnumerable<LabelValueOption> GetOptions<TEnum>() where TEnum : Enum {
        foreach (var item in typeof(TEnum).GetEnumValues()) {
            if (item is Enum v) {
                yield return new LabelValueOption { Id = v.GetHashCode(), Text = v.GetDescription() };
            }
        }
    }

    public static LabelValueOption[] GetDifficultyLevelOptions() {
        return LabelValueOptions.GetOrAdd(nameof(GetDifficultyLevelOptions), key => GetOptions<DifficultyLevel>().ToArray()[1..]);
    }

    public static LabelValueOption[] GetDifficultyLevelOptionsWithDefault() {
        return LabelValueOptions.GetOrAdd(nameof(GetDifficultyLevelOptionsWithDefault), key => [LabelValueOption.Default, .. GetDifficultyLevelOptions()]);
    }

    public static LabelValueOption[] GetTopicTypeOptions() {
        return LabelValueOptions.GetOrAdd(nameof(GetTopicTypeOptions), key => GetOptions<TopicType>().ToArray()[1..]);
    }

    public static LabelValueOption[] GetTopicTypeOptionsWithDefault() {
        return LabelValueOptions.GetOrAdd(nameof(GetTopicTypeOptionsWithDefault), key => [LabelValueOption.Default, .. GetTopicTypeOptions()]);
    }
}
