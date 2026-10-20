using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions;

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
        : base(
            validator,
            exceptionMapper,
            logger)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<BulkUpdateAlertActionsPayload>
        CreateExceptionFailure(
            ApplicationError error)
    {
        return BaseResult<BulkUpdateAlertActionsPayload>.Failure(
            error.Code,
            error.Message);
    }

    protected override BaseResult<BulkUpdateAlertActionsPayload>
        CreateValidationFailure(
            IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<
            BulkUpdateAlertActionsPayload>.FromValidationErrors(
                errors);
    }

    protected override async Task<
        BaseResult<BulkUpdateAlertActionsPayload>>
        ExecuteAsync(
            BulkUpdateAlertActionsCommand command,
            CancellationToken cancellationToken)
    {
        List<AlertActionChangePreview> changes;

        if (!command.Preview &&
            command.ConfirmedChanges is not null)
        {
            changes = command.ConfirmedChanges
                .Where(change =>
                    change.ChangedSettings.Count > 0)
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
            return BaseResult<
                BulkUpdateAlertActionsPayload>.Success(
                new BulkUpdateAlertActionsPayload(
                    SucceededCount: 0,
                    FailedCount: 0,
                    Changes: changes,
                    Results: []));
        }

        var updates = changes
            .Select(change =>
                new SavedSearchActionUpdate(
                    change.Id,
                    change.ChangedSettings))
            .ToList();

        var repositoryResult =
            await _savedSearchRepository
                .UpdateActionSettingsAsync(
                    updates,
                    cancellationToken);

        return BaseResult<
            BulkUpdateAlertActionsPayload>.Success(
            new BulkUpdateAlertActionsPayload(
                repositoryResult.SucceededCount,
                repositoryResult.FailedCount,
                changes,
                repositoryResult.Items));
    }

    private async Task<
        List<AlertActionChangePreview>>
        BuildChangesAsync(
            BulkUpdateAlertActionsCommand command,
            CancellationToken cancellationToken)
    {
        var query = new SavedSearchQuery(
            command.App,
            command.Owner,
            AlertsOnly: true,
            Paging: new());

        var page =
            await _savedSearchRepository.GetPageAsync(
                query,
                cancellationToken);

        var searches = page.Items;

        var changes =
            new List<AlertActionChangePreview>();

        foreach (var search in searches)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (command.SelectedSearches is { } selected &&
                !selected.Contains(search.Id))
            {
                continue;
            }

            var proposed =
                new Dictionary<string, string>(
                    search.ActionSettings.Values,
                    StringComparer.Ordinal);

            var changed =
                new Dictionary<string, string>(
                    StringComparer.Ordinal);

            foreach (var change in command.Changes)
            {

                if (!search.ActionSettings.Values.TryGetValue(
                        change.Key,
                        out var currentValue))
                {
                    continue;
                }


                if (!currentValue.Contains(change.Value.Substring, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var newValue = currentValue.Replace(
                    change.Value.Substring,
                    change.Value.Replacement,
                    StringComparison.OrdinalIgnoreCase);

                if (string.Equals(
                        currentValue,
                        newValue,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                proposed[change.Key] = newValue;
                changed[change.Key] = newValue;
            }

            if (changed.Count > 0)
            {
                changes.Add(
                    new AlertActionChangePreview(
                        search.Id,
                        search.ActionSettings.Values,
                        proposed,
                        changed));
            }
        }

        return changes;
    }
}