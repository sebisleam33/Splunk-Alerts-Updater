internal interface ISplunkSessionTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}
