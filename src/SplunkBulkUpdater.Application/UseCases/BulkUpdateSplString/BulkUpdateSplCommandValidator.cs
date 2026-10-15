using FluentValidation;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public sealed class BulkUpdateSplCommandValidator : AbstractValidator<BulkUpdateSplCommand>
{
    public BulkUpdateSplCommandValidator()
    {
        RuleFor(command => command.SplRegex)
            .NotEmpty().WithMessage("A search regex is required.")
            .Must(BeValidRegex).WithMessage("The search regex is not valid.");

        RuleFor(command => command).Custom(ValidateReplacements);
    }

    private static void ValidateReplacements(BulkUpdateSplCommand command,
        ValidationContext<BulkUpdateSplCommand> context)
    {
        IReadOnlyDictionary<string, string> replacements = command.GroupReplacements is { Count: > 0 }
            ? command.GroupReplacements
            : ParseLegacyReplacement(command.Replacement);
        if (replacements.Count == 0)
        {
            context.AddFailure(nameof(command.Replacement), "Enter at least one named group replacement.");
            return;
        }

        if (!BeValidRegex(command.SplRegex)) return;
        var groupNames = new Regex(command.SplRegex).GetGroupNames().ToHashSet(StringComparer.Ordinal);
        foreach (var groupName in replacements.Keys)
        {
            if (string.IsNullOrWhiteSpace(groupName) || int.TryParse(groupName, out _))
                context.AddFailure(nameof(command.GroupReplacements), "Replacement keys must be named regex groups.");
            else if (!groupNames.Contains(groupName))
                context.AddFailure(nameof(command.GroupReplacements), $"Regex does not contain a named group '{groupName}'.");
        }
    }

    private static IReadOnlyDictionary<string, string> ParseLegacyReplacement(string? replacement)
    {
        if (string.IsNullOrWhiteSpace(replacement)) return new Dictionary<string, string>();
        var separatorIndex = replacement.IndexOf('=');
        if (separatorIndex <= 0 || separatorIndex == replacement.Length - 1) return new Dictionary<string, string>();
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [replacement[..separatorIndex].Trim()] = replacement[(separatorIndex + 1)..]
        };
    }

    private static bool BeValidRegex(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return false;
        try { _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1)); return true; }
        catch (ArgumentException) { return false; }
    }
}
