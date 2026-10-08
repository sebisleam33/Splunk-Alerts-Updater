namespace SplunkBulkUpdater.Application.UseCases.Common
{
    public interface IApplicationExceptionMapper
    {
        ApplicationError Map(Exception exception);
    }
}