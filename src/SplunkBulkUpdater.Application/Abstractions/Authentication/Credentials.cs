namespace SplunkBulkUpdater.Application.Abstractions.Authentication;

public record Credentials(
    string Username,
    string Password);
