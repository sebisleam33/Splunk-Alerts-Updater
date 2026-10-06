using FluentValidation;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public sealed class GetAlertsCommandValidator
    : AbstractValidator<GetAlertsCommand>
{
    public GetAlertsCommandValidator()
    {
        RuleFor(command => command.SplRegex)
            .NotEmpty()
            .WithMessage("A search regex is required.")
            .Must(BeValidRegex)
            .WithMessage("The search regex is not valid.");
    }

    private static bool BeValidRegex(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        try
        {
            _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}