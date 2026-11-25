namespace SplunkBulkUpdater.Infrastructure.Splunk.Configuration;

public sealed class SplunkConnectionOptions
{
    public const string SectionName = "Splunk";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 8089;

    public TimeSpan SessionLifetime { get; set; } = TimeSpan.FromMinutes(55);

}
