using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.SplunkSdkImp;

public partial class SavedSearchSplunkSdkRepository
{
    public async Task<BulkUpdateResult> UpdateSplStringsAsync(
        IReadOnlyCollection<SavedSearchSearchUpdate> updates,
        CancellationToken cancellationToken)
    {
        List<UpdateResult> results = new List<UpdateResult>();

        var groups = updates.GroupBy(update => (update.Id.App, update.Id.Owner));

        foreach (var group in groups)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (_app, _owner) = group.Key;

            using var service = await _splunkServiceFactory
                .CreateAuthenticatedAsync(_authenticator.GetCredentials(), _app, _owner, cancellationToken);

            foreach (var update in group)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var savedSearch = service.SavedSearches.First(ss => ss.Name == update.Id.Name);
                //var savedSearch = await service.SavedSearches.GetAsync(update.Id.Name);

                var result = await savedSearch.UpdateAsync(update.SplSearchString);

                results.Add(new(update.Id, result));
            }
        }

        return new BulkUpdateResult(results);
    }
}
