using Splunk.Client;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SavedSearchModel =
    SplunkBulkUpdater.Application.Models.SavedSearches.SavedSearch;
using SplunkSavedSearch = Splunk.Client.SavedSearch;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRepository : ISavedSearchRepository
{
    public async Task<IReadOnlyList<SavedSearchModel>> GetAllAsync(
        string username,
        string password,
        SavedSearchQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var app = RequireNamespaceValue(query.App, nameof(query.App));
        var owner = RequireNamespaceValue(query.Owner, nameof(query.Owner));

        cancellationToken.ThrowIfCancellationRequested();

        using var service =
            await _splunkServiceFactory.CreateAuthenticatedAsync(
                username,
                password,
                app,
                owner,
                cancellationToken);

        await service.SavedSearches.GetAllAsync();

        var savedSearches = service.SavedSearches.ToAsyncEnumerable();

        if (query.AlertsOnly)
        {
            savedSearches = savedSearches.Where(IsAlert);
        }

        cancellationToken.ThrowIfCancellationRequested();

        return await savedSearches
            .Select(search =>
                new SavedSearchModel(
                    new SavedSearchId(
                        search.Name,
                        app,
                        owner),
                    search.Search))
            .ToListAsync(cancellationToken);
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

    private static bool IsAlert(SplunkSavedSearch search)
    {
        var hasAlertCondition =
            search.Alert.AlertType != AlertType.Always;

        var actions = search.Actions;

        var hasEnabledAction =
            actions.Email.IsEnabled ||
            actions.Rss.IsEnabled ||
            actions.Script.IsEnabled ||
            actions.SummaryIndex.IsEnabled ||
            actions.PopulateLookup.IsEnabled;

        return hasAlertCondition || hasEnabledAction;
    }
}