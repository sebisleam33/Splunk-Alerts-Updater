using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions
{
    internal static class AlertActionSettingsUpdater
    {
        public static bool IsMatch(
            AlertActionSettings current,
            AlertActionPatch match)
        {
            ArgumentNullException.ThrowIfNull(current);
            ArgumentNullException.ThrowIfNull(match);

            foreach (var expected in match.Values)
            {
                if (expected.Value is null)
                {
                    continue;
                }

                if (!current.Values.TryGetValue(
                        expected.Key,
                        out var currentValue))
                {
                    return false;
                }

                if (!string.Equals(
                        currentValue,
                        expected.Value,
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public static AlertActionMergeResult Apply(
            AlertActionSettings current,
            AlertActionPatch replacement)
        {
            ArgumentNullException.ThrowIfNull(current);
            ArgumentNullException.ThrowIfNull(replacement);

            var proposed = new Dictionary<string, string>(
                current.Values,
                StringComparer.Ordinal);

            var changed = new Dictionary<string, string>(
                StringComparer.Ordinal);

            foreach (var replacementValue in replacement.Values)
            {
                if (replacementValue.Value is null)
                {
                    continue;
                }

                if (proposed.TryGetValue(
                        replacementValue.Key,
                        out var currentValue) &&
                    string.Equals(
                        currentValue,
                        replacementValue.Value,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                proposed[replacementValue.Key] =
                    replacementValue.Value;

                changed[replacementValue.Key] =
                    replacementValue.Value;
            }

            return new AlertActionMergeResult(
                proposed,
                changed);
        }
    }

    internal sealed record AlertActionMergeResult(
        IReadOnlyDictionary<string, string> ProposedSettings,
        IReadOnlyDictionary<string, string> ChangedSettings);
}