namespace SplunkBulkUpdater.Application.UseCases.Common
{
    public enum ResultStatus
    {
        Ok,
        ValidationError,
        Error
    }

    public enum ErrorCode
    {
        None,
        ValidationFailed,
        AuthenticationRequired,
        RegexEvaluationTimeout,
        OperationFailed
    }

    public sealed record ValidationError(
        string PropertyName,
        string Code,
        string Message);

    public record BaseResult(
        ResultStatus Status,
        ErrorCode ErrorCode,
        string? Message = null,
        IReadOnlyList<ValidationError>? ValidationErrors = null)
    {
        public bool IsSuccess => Status == ResultStatus.Ok;

        public static BaseResult Success() =>
            new(ResultStatus.Ok, ErrorCode.None);

        public static BaseResult Failure(
            ErrorCode errorCode,
            string message) =>
            new(
                ResultStatus.Error,
                errorCode,
                message);

        public static BaseResult FromValidationErrors(
            IEnumerable<ValidationError> errors) =>
            new(
                ResultStatus.ValidationError,
                ErrorCode.ValidationFailed,
                ValidationErrors: errors.ToArray());
    }

    public record BaseResult<T>(
        T? Payload,
        ResultStatus Status,
        ErrorCode ErrorCode,
        string? Message = null,
        IReadOnlyList<ValidationError>? ValidationErrors = null)
        : BaseResult(Status, ErrorCode, Message, ValidationErrors)
    {
        public static new BaseResult<T> FromValidationErrors(
            IEnumerable<ValidationError> errors) =>
            new(
                default,
                ResultStatus.ValidationError,
                ErrorCode.ValidationFailed,
                ValidationErrors: errors.ToArray());

        public static BaseResult<T> Failure(
            ErrorCode errorCode,
            string message) =>
            new(
                default,
                ResultStatus.Error,
                errorCode,
                message);

        public static BaseResult<T> Success(T payload) =>
            new(
                payload,
                ResultStatus.Ok,
                ErrorCode.None);
    }
}
