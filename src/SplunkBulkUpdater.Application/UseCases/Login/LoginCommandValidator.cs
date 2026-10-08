using FluentValidation;
using SplunkBulkUpdater.Application.UseCases.LogIn;

namespace SplunkBulkUpdater.Application.UseCases.Login;

internal class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .WithMessage("Username is required.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
