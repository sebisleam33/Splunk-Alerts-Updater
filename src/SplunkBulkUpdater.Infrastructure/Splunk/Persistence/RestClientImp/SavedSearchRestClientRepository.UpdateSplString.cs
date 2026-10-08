using System.Diagnostics;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRestClientRepository
{
    public async Task<BulkUpdateResult> UpdateSplStringsAsync(
        IReadOnlyCollection<SavedSearchSearchUpdate> updates,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updates);

        var results = new List<UpdateResult>();

        foreach (var update in updates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var id = update.Id;

            try
            {
                Debug.WriteLine(
                    $"[SPLUNK] Retrieving: {id.Name}");

                var savedSearch = await _restClient.GetAsync(
                    id,
                    cancellationToken);

                if (savedSearch.SplSearchString ==
                    update.SplSearchString)
                {
                    Debug.WriteLine(
                        $"[SPLUNK] No changes: {id.Name}");

                    results.Add(new UpdateResult(
                        id,
                        true));

                    continue;
                }

                Debug.WriteLine(
                    $"[SPLUNK] Updating: {id.Name}");

                await _restClient.UpdateAsync(
                    id,
                    update.SplSearchString,
                    cancellationToken);

                // Verify the persisted SPL value.
                var verified = await _restClient.GetAsync(
                    id,
                    cancellationToken);

                if (verified.SplSearchString !=
                    update.SplSearchString)
                {
                    throw new InvalidOperationException(
                        "Splunk did not persist the expected SPL query.");
                }

                results.Add(new UpdateResult(
                    id,
                    true));

                Debug.WriteLine(
                    $"[SPLUNK] SUCCESS: {id.Name}");
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[SPLUNK] FAILED: {id.Name}");

                Debug.WriteLine(ex.ToString());

                results.Add(new UpdateResult(
                    id,
                    false,
                    ex.Message));
            }
        }

        return new BulkUpdateResult(results);
    }
}
