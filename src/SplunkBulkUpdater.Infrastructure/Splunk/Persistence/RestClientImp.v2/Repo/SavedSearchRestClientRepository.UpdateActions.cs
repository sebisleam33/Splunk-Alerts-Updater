using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.Repo;

internal sealed partial class SavedSearchRestClientRepository
{
    public async Task<BulkUpdateResult> UpdateActionSettingsAsync(
        IReadOnlyCollection<SavedSearchActionUpdate> updates,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updates);

        var results = new List<UpdateResult>(updates.Count);

        foreach (var update in updates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _restClient.UpdateActionSettingsAsync(
                    update,
                    cancellationToken);

                results.Add(new UpdateResult(
                    update.Id,
                    Succeeded: true));
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Could not update alert actions for saved search {Name}, app {App}, owner {Owner}.",
                    update.Id.Name,
                    update.Id.App,
                    update.Id.Owner);

                results.Add(new UpdateResult(
                    update.Id,
                    Succeeded: false,
                    exception.Message));
            }
        }

        return new BulkUpdateResult(results);
    }
}