namespace SplunkBulkUpdater.Application.UseCases.Common
{
    public sealed record ApplicationError(
        ErrorCode Code,
        string Message);
}