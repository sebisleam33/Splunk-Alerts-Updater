using SplunkBulkUpdater.Application.Models.ChangeTypes;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.Repo;

internal partial class SavedSearchRestClientRepository
{
    public async Task<ChangeTypeFields> GetChangeTypeFieldSchema(ChangeType changeType, CancellationToken cancellationToken)
    {
        string changeTypeAsString = changeType.ToString().ToLower();

        var skeleton = await _restClient.GetSearchSkeleton(changeTypeAsString, cancellationToken);

        var prefix = $"action.{changeTypeAsString}";

        var skeletonDict = skeleton
            .Where(field =>
                field.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                field.Length > prefix.Length)
            .ToDictionary(
                field => field,
                ToLabel);

        return new ChangeTypeFields(changeType, skeletonDict);
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
