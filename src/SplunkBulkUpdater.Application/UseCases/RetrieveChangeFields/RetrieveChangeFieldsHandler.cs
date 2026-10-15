using System.Collections.ObjectModel;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveChangeFields
{
    public sealed class RetrieveChangeFieldsHandler
        : ICommandHandler<
            RetrieveChangeFieldsCommand,
            BaseResult<ChangeTypeFields>>
    {
        public Task<BaseResult<ChangeTypeFields>> HandleAsync(
            RetrieveChangeFieldsCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Enum.IsDefined(command.ChangeType))
            {
                return Task.FromResult(
                    BaseResult<ChangeTypeFields>.Failure(
                        ErrorCode.ValidationFailed,
                        $"Unsupported change type: {command.ChangeType}."));
            }

            var fields = command.ChangeType switch
            {
                ChangeType.SPL => CreateFields(
                    ("search", "SPL")),

                ChangeType.Email => CreateFields(
                    ("action.email.to", "Email To"),
                    ("action.email.subject", "Email Subject"),
                    ("action.email.message", "Email Message")),

                ChangeType.Webhook => CreateFields(
                    ("action.webhook.param.url", "Webhook URL")),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(command.ChangeType),
                    command.ChangeType,
                    "Unsupported change type.")
            };

            var payload = new ChangeTypeFields(
                command.ChangeType,
                fields);

            return Task.FromResult(
                BaseResult<ChangeTypeFields>.Success(payload));
        }

        private static IReadOnlyDictionary<string, string> CreateFields(
            params (string Key, string Label)[] fields)
        {
            return new ReadOnlyDictionary<string, string>(
                fields.ToDictionary(
                    field => field.Key,
                    field => field.Label,
                    StringComparer.Ordinal));
        }
    }
}