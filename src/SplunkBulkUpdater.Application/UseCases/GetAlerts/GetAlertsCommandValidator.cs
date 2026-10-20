using FluentValidation;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public sealed class GetAlertsCommandValidator
    : AbstractValidator<GetAlertsCommand>
{
    public GetAlertsCommandValidator()
    {
        RuleFor(command => command.SplRegex)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The first search regex is required.")
            .Must(BeValidRegex)
            .WithMessage("The first search regex is not valid.");

        RuleForEach(command =>
                command.AdditionalSplRegexes ?? Array.Empty<string>())
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("An additional search regex cannot be empty.")
            .Must(BeValidRegex)
            .WithMessage("An additional search regex is not valid.");
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