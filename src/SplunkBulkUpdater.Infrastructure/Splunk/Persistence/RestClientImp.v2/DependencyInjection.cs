using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient.Authentication;
using SplunkRestV2 = SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient.SplunkRestClient;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2;

internal static class DependencyInjection
{
    internal static IServiceCollection AddRestClientV2(this IServiceCollection services)
    {
        services.AddHttpClient("SplunkLogin")
            .ConfigureHttpClient(ConfigureSplunkBaseAddress);

        services.AddSingleton<ISplunkSessionTokenProvider, SplunkSessionTokenProvider>();
        services.AddTransient<SplunkTokenHandler>();

        services.AddHttpClient<ISplunkRestClient, SplunkRestV2>()
            .ConfigureHttpClient(ConfigureSplunkBaseAddress)
            .AddHttpMessageHandler<SplunkTokenHandler>();

        return services;
    }

    private static void ConfigureSplunkBaseAddress(
        IServiceProvider serviceProvider,
        HttpClient client)
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<SplunkConnectionOptions>>()
            .Value;

        client.BaseAddress =
            new Uri($"https://{options.Host}:{options.Port}/");
    }
}
