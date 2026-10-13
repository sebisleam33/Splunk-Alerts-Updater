using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient
{
    internal partial class SplunkRestClient
    {
        public async Task UpdateActionSettingsAsync(
            SavedSearchActionUpdate update,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(update);

            if (update.ChangedValues.Count == 0)
            {
                return;
            }

            var path = BuildEntityPath(update.Id);

            using var request = CreateRequest(
                HttpMethod.Post,
                path);

            request.Content = new FormUrlEncodedContent(
                update.ChangedValues);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            await EnsureSuccessAsync(
                response,
                cancellationToken);
        }
    }
}