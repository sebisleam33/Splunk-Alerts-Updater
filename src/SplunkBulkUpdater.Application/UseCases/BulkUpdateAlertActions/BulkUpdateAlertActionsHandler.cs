using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions
{
    public sealed class BulkUpdateAlertActionsHandler
        : AbstractCommandHandler<
            BulkUpdateAlertActionsCommand,
            BaseResult<BulkUpdateAlertActionsPayload>>
    {
        private readonly ISavedSearchRepository _savedSearchRepository;

        public BulkUpdateAlertActionsHandler(
            IValidator<BulkUpdateAlertActionsCommand> validator,
            ISavedSearchRepository savedSearchRepository,
            IApplicationExceptionMapper exceptionMapper,
            ILogger<BulkUpdateAlertActionsHandler> logger)
            : base(validator, exceptionMapper, logger)
        {
            _savedSearchRepository = savedSearchRepository;
        }

        protected override BaseResult<BulkUpdateAlertActionsPayload>
            CreateExceptionFailure(ApplicationError error)
        {
            return BaseResult<BulkUpdateAlertActionsPayload>.Failure(
                error.Code,
                error.Message);
        }

        protected override BaseResult<BulkUpdateAlertActionsPayload>
            CreateValidationFailure(
                IReadOnlyList<ValidationError> errors)
        {
            return BaseResult<BulkUpdateAlertActionsPayload>
                .FromValidationErrors(errors);
        }

        protected override async Task<
            BaseResult<BulkUpdateAlertActionsPayload>> ExecuteAsync(
            BulkUpdateAlertActionsCommand command,
            CancellationToken cancellationToken)
        {
            List<AlertActionChangePreview> changes;

            if (!command.Preview &&
                command.ConfirmedChanges is not null)
            {
                changes = command.ConfirmedChanges
                    .Where(change => change.ChangedSettings.Count > 0)
                    .ToList();
            }
            else
            {
                changes = await BuildChangesAsync(
                    command,
                    cancellationToken);
            }

            if (command.Preview)
            {
                return BaseResult<BulkUpdateAlertActionsPayload>.Success(
                    new BulkUpdateAlertActionsPayload(
                        SucceededCount: 0,
                        FailedCount: 0,
                        Changes: changes,
                        Results: []));
            }

            var updates = changes
                .Select(change => new SavedSearchActionUpdate(
                    change.Id,
                    change.ChangedSettings))
                .ToList();

            var repositoryResult =
                await _savedSearchRepository.UpdateActionSettingsAsync(
                    updates,
                    cancellationToken);

            return BaseResult<BulkUpdateAlertActionsPayload>.Success(
                new BulkUpdateAlertActionsPayload(
                    repositoryResult.SucceededCount,
                    repositoryResult.FailedCount,
                    changes,
                    repositoryResult.Items));
        }

        private async Task<List<AlertActionChangePreview>>
            BuildChangesAsync(
                BulkUpdateAlertActionsCommand command,
                CancellationToken cancellationToken)
        {
            var query = new SavedSearchQuery(
                command.App,
                command.Owner,
                AlertsOnly: true);

            var searches = await _savedSearchRepository.GetAllAsync(
                query,
                cancellationToken);

            var changes = new List<AlertActionChangePreview>();

            foreach (var search in searches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!AlertActionSettingsUpdater.IsMatch(
                        search.ActionSettings,
                        command.Match))
                {
                    continue;
                }

                var mergeResult = AlertActionSettingsUpdater.Apply(
                    search.ActionSettings,
                    command.Replacement);

                if (mergeResult.ChangedSettings.Count == 0)
                {
                    continue;
                }

                changes.Add(new AlertActionChangePreview(
                    search.Id,
                    search.ActionSettings.Values,
                    mergeResult.ProposedSettings,
                    mergeResult.ChangedSettings));
            }

            return changes;
        }
    }
}