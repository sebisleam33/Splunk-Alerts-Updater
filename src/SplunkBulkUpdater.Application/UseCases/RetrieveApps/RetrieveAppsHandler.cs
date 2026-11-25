using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveApps;

public sealed class RetrieveAppsHandler : ICommandHandler<RetrieveAppsCommand, BaseResult<RetrieveAppsPayload>>
{
    private readonly ISavedSearchRepository _repository;

    public RetrieveAppsHandler(ISavedSearchRepository repository) => _repository = repository;

    public async Task<BaseResult<RetrieveAppsPayload>> HandleAsync(
        RetrieveAppsCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var names = await _repository.GetAppsAsync(cancellationToken);
        var apps = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
            names.ToDictionary(name => name, name => name, StringComparer.OrdinalIgnoreCase));
        return BaseResult<RetrieveAppsPayload>.Success(new RetrieveAppsPayload(apps));
    }
}
