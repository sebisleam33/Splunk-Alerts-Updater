using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SavedSearchModel =
    SplunkBulkUpdater.Application.Models.SavedSearches.SavedSearch;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRepository : ISavedSearchRepository
{
    public async Task<IReadOnlyList<SavedSearchModel>> GetAllAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var app = RequireNamespaceValue(query.App, nameof(query.App));
        var owner = RequireNamespaceValue(query.Owner, nameof(query.Owner));

        cancellationToken.ThrowIfCancellationRequested();

        using var service =
            await _splunkServiceFactory.CreateAuthenticatedAsync(
                _authenticator.GetCredentials(),
                app,
                owner,
                cancellationToken);

        await service.SavedSearches.GetAllAsync();

        cancellationToken.ThrowIfCancellationRequested();

        return service.SavedSearches
            .Select(search =>
                new SavedSearchModel(
                    new SavedSearchId(
                        search.Name,
                        app,
                        owner),
                    search.Search))
            .ToList();
    }

    private static string RequireNamespaceValue(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "An app and owner are required to retrieve saved searches.",
                parameterName);
        }

        return value.Trim();
    }
}