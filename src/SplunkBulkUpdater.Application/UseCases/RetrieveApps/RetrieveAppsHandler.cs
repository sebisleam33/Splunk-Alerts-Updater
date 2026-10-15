using System.Collections.ObjectModel;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveApps
{
    public sealed class RetrieveAppsHandler
        : ICommandHandler<
            RetrieveAppsCommand,
            BaseResult<RetrieveAppsPayload>>
    {
        public Task<BaseResult<RetrieveAppsPayload>> HandleAsync(
            RetrieveAppsCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var apps = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    [SavedSearchDefaults.App] = SavedSearchDefaults.App
                });

            var payload = new RetrieveAppsPayload(apps);

            return Task.FromResult(
                BaseResult<RetrieveAppsPayload>.Success(payload));
        }
    }
}