namespace SplunkBulkUpdater.Infrastructure.Splunk.Configuration;

public sealed class SplunkConnectionOptions
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 8089;
}
