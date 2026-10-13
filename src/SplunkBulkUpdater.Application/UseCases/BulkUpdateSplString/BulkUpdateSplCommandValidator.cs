using FluentValidation;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public sealed class BulkUpdateSplCommandValidator
    : AbstractValidator<BulkUpdateSplCommand>
{
    public BulkUpdateSplCommandValidator()
    {
        RuleFor(command => command.SplRegex)
            .NotEmpty()
            .WithMessage("A search regex is required.")
            .Must(BeValidRegex)
            .WithMessage("The search regex is not valid.");

        RuleFor(command => command.Replacement)
            .NotEmpty()
            .WithMessage("A replacement in the format groupName=newValue is required.")
            .Must(BeValidReplacement)
            .WithMessage("The replacement must use the format groupName=newValue.")
            .Must((command, replacement) =>
                ReferencesExistingNamedGroup(command.SplRegex, replacement))
            .WithMessage("The replacement must reference a named group in the search regex.");
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

    private static bool BeValidReplacement(string? replacement)
    {
        if (string.IsNullOrWhiteSpace(replacement))
            return false;

        var separatorIndex = replacement.IndexOf('=');

        return separatorIndex > 0 &&
               separatorIndex < replacement.Length - 1;
    }

    private static bool ReferencesExistingNamedGroup(
        string? pattern,
        string? replacement)
    {
        if (string.IsNullOrWhiteSpace(pattern) ||
            string.IsNullOrWhiteSpace(replacement))
        {
            return false;
        }

        var separatorIndex = replacement.IndexOf('=');

        if (separatorIndex <= 0 || separatorIndex == replacement.Length - 1)
            return false;

        var groupName = replacement[..separatorIndex].Trim();

        if (groupName.Length == 0 ||
            int.TryParse(groupName, out _)) // Require a named, not numbered, group.
        {
            return false;
        }

        try
        {
            var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
            return regex.GetGroupNames().Contains(groupName);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}