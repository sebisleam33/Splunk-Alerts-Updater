using FluentValidation;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public sealed class GetAlertsCommandValidator
    : AbstractValidator<GetAlertsCommand>
{
    public GetAlertsCommandValidator()
    {
        RuleFor(command => command.App)
            .NotEmpty()
            .WithMessage("An app is required.");

        RuleFor(command => command.Owner)
            .NotEmpty()
            .WithMessage("An owner is required.");

        RuleFor(command => command.Paging)
            .NotNull()
            .WithMessage("Paging information is required.");

        RuleForEach(command => command.SearchStringFilters)
            .Must(filter =>
                !string.IsNullOrWhiteSpace(filter.Key) &&
                !string.IsNullOrWhiteSpace(filter.Value))
            .WithMessage("Search filter names and values cannot be empty.");
    }
}