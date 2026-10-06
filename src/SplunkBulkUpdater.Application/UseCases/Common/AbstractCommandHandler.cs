using FluentValidation;
using SplunkBulkUpdater.Application.Abstractions.Messaging;

namespace SplunkBulkUpdater.Application.UseCases.Common;

public abstract class AbstractCommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : BaseResult
{
    private readonly IValidator<TCommand> _validator;

    protected AbstractCommandHandler(IValidator<TCommand> validator)
    {
        _validator = validator;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        var validationResult =
            await _validator.ValidateAsync(command, cancellationToken);

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

        return await ExecuteAsync(command, cancellationToken);
    }
    protected abstract Task<TResult> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
    protected abstract TResult CreateValidationFailure(
    IReadOnlyList<ValidationError> errors);
}
