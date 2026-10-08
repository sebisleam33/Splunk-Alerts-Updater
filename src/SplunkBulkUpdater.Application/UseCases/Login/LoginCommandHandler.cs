using FluentValidation;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.LogIn;

namespace SplunkBulkUpdater.Application.UseCases.Login;

public class LoginCommandHandler : AbstractCommandHandler<LoginCommand, BaseResult>
{
    private readonly IAuthenticator _authenticator;

    public LoginCommandHandler(IValidator<LoginCommand> validator, IAuthenticator authenticator) : base(validator)
    {
        _authenticator = authenticator;
    }

    protected override BaseResult CreateValidationFailure(IReadOnlyList<ValidationError> errors)
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
