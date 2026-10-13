using System.Collections.ObjectModel;

namespace SplunkBulkUpdater.Application.Models.SavedSearches
{
    public sealed class AlertActionSettings
    {
        public static AlertActionSettings Empty { get; } = new([]);

        public AlertActionSettings(
            IEnumerable<KeyValuePair<string, string>> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            Values = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(
                    values,
                    StringComparer.Ordinal));
        }

        public IReadOnlyDictionary<string, string> Values { get; }
    }
}