using FluentValidation;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public sealed class BulkUpdateSplCommandValidator
    : AbstractValidator<BulkUpdateSplCommand>
{
    public BulkUpdateSplCommandValidator()
    {
        RuleFor(command => command.App)
            .NotEmpty()
            .WithMessage("An app is required.");

        RuleFor(command => command.Owner)
            .NotEmpty()
            .WithMessage("An owner is required.");

        RuleFor(command => command.ReplacementPair.Substring)
            .NotEmpty()
            .WithMessage("Enter the text to search for.");

        RuleFor(command => command.ReplacementPair.Replacement)
            .NotNull()
            .WithMessage("Enter replacement text.");

        RuleFor(command => command.SelectedSearches)
            .NotNull()
            .WithMessage("Select at least one saved search.")
            .Must(searches => searches is { Count: > 0 })
            .WithMessage("Select at least one saved search.");
    }
}