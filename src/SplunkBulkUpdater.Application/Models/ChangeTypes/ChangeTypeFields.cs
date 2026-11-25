using System.Collections.ObjectModel;

namespace SplunkBulkUpdater.Application.Models.ChangeTypes
{
    public sealed class ChangeTypeFields
    {
        public ChangeType ChangeType { get; }

        public IReadOnlyDictionary<string, string> Fields { get; }

        public ChangeTypeFields(
            ChangeType changeType,
            IReadOnlyDictionary<string, string> fields)
        {
            ArgumentNullException.ThrowIfNull(fields);

            ChangeType = changeType;

            Fields = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(
                    fields,
                    StringComparer.Ordinal));
        }
    }
}