namespace SplunkBulkUpdater.WPF.ViewModels;

/// <summary>Stores the authenticated Splunk username for owner-scoped queries.</summary>
public sealed class SignedInUserContext
{
    public string Username { get; private set; } = string.Empty;

    public void SetUsername(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        Username = username.Trim();
    }

    public void Clear()
    {
        Username = string.Empty;
    }
}