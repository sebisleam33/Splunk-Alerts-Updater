using FluentValidation;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveChangeFields
{
    public sealed class RetrieveChangeFieldsCommandValidator
        : AbstractValidator<RetrieveChangeFieldsCommand>
    {
        public RetrieveChangeFieldsCommandValidator()
        {
            RuleFor(command => command.ChangeType)
                .IsInEnum()
                .WithMessage("A supported change type is required.");
        }
    }
}