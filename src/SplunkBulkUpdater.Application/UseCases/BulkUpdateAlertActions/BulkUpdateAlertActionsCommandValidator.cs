using FluentValidation;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions
{
    public sealed class BulkUpdateAlertActionsCommandValidator
        : AbstractValidator<BulkUpdateAlertActionsCommand>
    {
        private const string WebhookUrlKey =
            "action.webhook.param.url";

        public BulkUpdateAlertActionsCommandValidator()
        {
            RuleFor(command => command.App)
                .NotEmpty()
                .WithMessage("A Splunk application is required.");

            RuleFor(command => command.Owner)
                .NotEmpty()
                .WithMessage("A Splunk owner is required.");

            RuleFor(command => command)
                .Custom(ValidateCommand);
        }

        private static void ValidateCommand(
            BulkUpdateAlertActionsCommand command,
            ValidationContext<BulkUpdateAlertActionsCommand> context)
        {
            if (command.Match is null)
            {
                context.AddFailure(
                    nameof(command.Match),
                    "Action match settings are required.");
            }
            else
            {
                ValidatePatch(
                    command.Match,
                    nameof(command.Match),
                    requireNonNullValue: true,
                    context);
            }

            if (command.Replacement is null)
            {
                context.AddFailure(
                    nameof(command.Replacement),
                    "Action replacement settings are required.");
            }
            else
            {
                ValidatePatch(
                    command.Replacement,
                    nameof(command.Replacement),
                    requireNonNullValue: true,
                    context);
            }
        }

        private static void ValidatePatch(
            AlertActionPatch patch,
            string propertyName,
            bool requireNonNullValue,
            ValidationContext<BulkUpdateAlertActionsCommand> context)
        {
            if (patch.Values.Count == 0)
            {
                context.AddFailure(
                    propertyName,
                    "At least one action setting is required.");

                return;
            }

            if (requireNonNullValue &&
                patch.Values.All(item => item.Value is null))
            {
                context.AddFailure(
                    propertyName,
                    "At least one action setting must have a non-null value.");
            }

            foreach (var item in patch.Values)
            {
                if (!IsSupportedKey(item.Key))
                {
                    context.AddFailure(
                        propertyName,
                        $"Action setting '{item.Key}' is not supported.");
                }

                if (item.Key.Equals(
                        WebhookUrlKey,
                        StringComparison.Ordinal) &&
                    item.Value is not null &&
                    !IsValidHttpUrl(item.Value))
                {
                    context.AddFailure(
                        propertyName,
                        $"'{WebhookUrlKey}' must contain an absolute HTTP or HTTPS URL.");
                }
            }
        }

        private static bool IsSupportedKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return key.Equals(
                       "actions",
                       StringComparison.Ordinal) ||
                   key.Equals(
                       "action.email",
                       StringComparison.Ordinal) ||
                   key.StartsWith(
                       "action.email.",
                       StringComparison.Ordinal) ||
                   key.Equals(
                       "action.webhook",
                       StringComparison.Ordinal) ||
                   key.StartsWith(
                       "action.webhook.",
                       StringComparison.Ordinal);
        }

        private static bool IsValidHttpUrl(string value)
        {
            return Uri.TryCreate(
                       value,
                       UriKind.Absolute,
                       out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp ||
                    uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}