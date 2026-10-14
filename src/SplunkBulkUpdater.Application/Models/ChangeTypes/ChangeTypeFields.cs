namespace SplunkBulkUpdater.Application.Models.ChangeTypes;

public class ChangeTypeFields
{
    public IReadOnlyDictionary<string, string> Fields { get; }
    public ChangeType ChangeType { get; set; }

    public ChangeTypeFields(IReadOnlyDictionary<string, string> fields) => Fields = fields;
}
