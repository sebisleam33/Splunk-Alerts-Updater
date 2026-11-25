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
            if (command.Changes is null || command.Changes.Count == 0)
            {
                context.AddFailure(nameof(command.Changes), "Select at least one change field.");
                return;
            }

            foreach (var (key, replacement) in command.Changes)
            {
                if (!IsSupportedKey(key, command.ChangeType))
                    context.AddFailure(nameof(command.Changes), $"Action setting '{key}' is not supported.");
                if (replacement is null || string.IsNullOrWhiteSpace(replacement.Substring))
                    context.AddFailure(nameof(command.Changes), $"Enter a match regex for '{key}'.");
                else
                {
                    try { _ = new System.Text.RegularExpressions.Regex(replacement.Substring, System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2)); }
                    catch (ArgumentException ex) { context.AddFailure(nameof(command.Changes), $"Invalid regex for '{key}': {ex.Message}"); }
                }
                if (key.Equals(WebhookUrlKey, StringComparison.Ordinal) && replacement?.Replacement is not null &&
                    !IsValidHttpUrl(replacement.Replacement))
                    context.AddFailure(nameof(command.Changes), $"'{WebhookUrlKey}' must contain an absolute HTTP or HTTPS URL.");
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

        private static bool IsSupportedKey(string key, SplunkBulkUpdater.Application.Models.ChangeTypes.ChangeType changeType)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return key.Equals("actions", StringComparison.Ordinal) ||
                   (changeType == SplunkBulkUpdater.Application.Models.ChangeTypes.ChangeType.Email &&
                    (key.Equals("action.email", StringComparison.Ordinal) || key.StartsWith("action.email.", StringComparison.Ordinal))) ||
                   (changeType == SplunkBulkUpdater.Application.Models.ChangeTypes.ChangeType.Webhook &&
                    (key.Equals("action.webhook", StringComparison.Ordinal) || key.StartsWith("action.webhook.", StringComparison.Ordinal)));
        }

        private static bool IsSupportedKey(string key) =>
            IsSupportedKey(key, SplunkBulkUpdater.Application.Models.ChangeTypes.ChangeType.Email) ||
            IsSupportedKey(key, SplunkBulkUpdater.Application.Models.ChangeTypes.ChangeType.Webhook);

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
