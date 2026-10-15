using SplunkBulkUpdater.Application.Models.ChangeTypes;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.Repo;

internal partial class SavedSearchRestClientRepository
{
    public async Task<ChangeTypeFields> GetChangeTypeFieldSchema(ChangeType changeType, CancellationToken cancellationToken)
    {
        string changeTypeAsString = changeType.ToString().ToLower();

        var scheleton = await _restClient.GetSearchSkeleton(changeTypeAsString, cancellationToken);

        var scheletonDict = scheleton
            .Where(field => field.StartsWith(changeTypeAsString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
            field => field,
            ToLabel);

        return new ChangeTypeFields(changeType, scheletonDict);
    }

    private static string ToLabel(string apiField)
    {
        ArgumentNullException.ThrowIfNull(apiField);

        return string.Join(
            ' ',
            apiField.Split('.')
                .Skip(2)
                .SelectMany(part => part.Split(
                    '_',
                    StringSplitOptions.RemoveEmptyEntries))
                .Select(Capitalize));
    }

    private static string Capitalize(string word) =>
        word.Length == 0
            ? word
            : char.ToUpperInvariant(word[0]) + word[1..];
}
