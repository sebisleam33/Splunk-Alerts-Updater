using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.Repo;

internal sealed partial class SavedSearchRestClientRepository
    : ISavedSearchRepository
{
    private readonly ISplunkRestClient _restClient;
    private readonly ILogger<SavedSearchRestClientRepository> _logger;

    public SavedSearchRestClientRepository(
        ISplunkRestClient restClient,
        ILogger<SavedSearchRestClientRepository> logger)
    {
        _restClient = restClient;
        _logger = logger;
    }


}