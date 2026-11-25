namespace SplunkBulkUpdater.Application.Abstractions.Authentication.Exceptions;

public sealed class AuthenticationRequiredException : Exception
{
    public AuthenticationRequiredException()
        : base("Authentication is required before performing this operation.")
    {
    }
}