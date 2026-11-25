using FluentValidation;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.LogIn;
using Microsoft.Extensions.Logging;

namespace SplunkBulkUpdater.Application.UseCases.Login;

public class LoginCommandHandler : AbstractCommandHandler<LoginCommand, BaseResult>
{
    private readonly IAuthenticator _authenticator;

    public LoginCommandHandler(
        IValidator<LoginCommand> validator,
        IAuthenticator authenticator,
        IApplicationExceptionMapper exceptionMapper,
        ILogger<LoginCommandHandler> logger)
        : base(validator, exceptionMapper, logger)
    {
        _authenticator = authenticator;
    }

    protected override BaseResult CreateExceptionFailure(
    ApplicationError error)
    {
        return BaseResult.Failure(
            error.Code,
            error.Message);
    }

    protected override BaseResult CreateValidationFailure(
    IReadOnlyList<ValidationError> errors)
    {
        return BaseResult.FromValidationErrors(errors);
    }

    protected override async Task<BaseResult> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        Credentials credentials = new(command.Username, command.Password);

        await _authenticator.Authenticate(credentials);

        return BaseResult.Success();
    }
}
