using SplunkBulkUpdater.Application.Abstractions.Authentication.Exceptions;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.Common
{
    public sealed class ApplicationExceptionMapper: IApplicationExceptionMapper
    {
        public ApplicationError Map(Exception exception)
        {
            return exception switch
            {
                AuthenticationRequiredException =>
                    new(
                        ErrorCode.AuthenticationRequired,
                        "Authentication is required before performing this operation."),

                RegexMatchTimeoutException =>
                    new(
                        ErrorCode.RegexEvaluationTimeout,
                        "The search pattern took too long to evaluate."),

                _ =>
                    new(
                        ErrorCode.OperationFailed,
                        "The operation could not be completed.")
            };
        }
    }
}