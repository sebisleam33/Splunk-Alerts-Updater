namespace SplunkBulkUpdater.Application.Abstractions.Authentication;

public interface IAuthenticator
{
    Task Authenticate(Credentials credentials);
    Credentials GetCredentials();
}
