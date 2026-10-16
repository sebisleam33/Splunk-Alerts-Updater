namespace SplunkBulkUpdater.Application.Abstractions.Authentication;

public interface IAuthenticator
{
    Task Authenticate(Credentials credentials, CancellationToken cancellationToken = default);
    Credentials GetCredentials();

    void SignOut();
}
