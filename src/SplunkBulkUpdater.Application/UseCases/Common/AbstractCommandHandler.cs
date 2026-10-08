using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Messaging;

namespace SplunkBulkUpdater.Application.UseCases.Common
{
    public abstract class AbstractCommandHandler<TCommand, TResult>
        : ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
        where TResult : BaseResult
    {
        private readonly IValidator<TCommand> _validator;
        private readonly IApplicationExceptionMapper _exceptionMapper;
        private readonly ILogger _logger;

        protected AbstractCommandHandler(
            IValidator<TCommand> validator,
            IApplicationExceptionMapper exceptionMapper,
            ILogger logger)
        {
            _validator = validator;
            _exceptionMapper = exceptionMapper;
            _logger = logger;
        }

        public async Task<TResult> HandleAsync(
            TCommand command,
            CancellationToken cancellationToken)
        {
            var validationResult =
                await _validator.ValidateAsync(
                    command,
                    cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(error => new ValidationError(
                        error.PropertyName,
                        error.ErrorCode,
                        error.ErrorMessage))
                    .ToArray();

                return CreateValidationFailure(errors);
            }

            try
            {
                return await ExecuteAsync(
                    command,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while executing command {CommandType}.",
                    typeof(TCommand).Name);

                var error = _exceptionMapper.Map(exception);

                return CreateExceptionFailure(error);
            }
        }

        protected abstract Task<TResult> ExecuteAsync(
            TCommand command,
            CancellationToken cancellationToken);

        protected abstract TResult CreateValidationFailure(
            IReadOnlyList<ValidationError> errors);

        protected abstract TResult CreateExceptionFailure(
            ApplicationError error);
    }
}