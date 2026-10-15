
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRestClientRepository : ISavedSearchRepository
{
    private readonly SplunkRestClient _restClient;

    public SavedSearchRestClientRepository(SplunkRestClient restClient)
    {
        _restClient = restClient;
    }

    public Task<ChangeTypeFields> GetChangeTypeFieldSchema(ChangeType changeType, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<BulkUpdateResult> UpdateActionSettingsAsync(IReadOnlyCollection<SavedSearchActionUpdate> updates, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
