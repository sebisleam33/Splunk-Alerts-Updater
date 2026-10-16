using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public sealed class BulkUpdateSplHandler
    : AbstractCommandHandler<
        BulkUpdateSplCommand,
        BaseResult<BulkUpdateSplPayload>>
{
    private readonly ISavedSearchRepository _savedSearchRepository;

    public BulkUpdateSplHandler(
        IValidator<BulkUpdateSplCommand> validator,
        ISavedSearchRepository savedSearchRepository,
        IApplicationExceptionMapper exceptionMapper,
        ILogger<BulkUpdateSplHandler> logger)
        : base(
            validator,
            exceptionMapper,
            logger)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<BulkUpdateSplPayload>
        CreateExceptionFailure(
            ApplicationError error) =>
        BaseResult<BulkUpdateSplPayload>.Failure(
            error.Code,
            error.Message);

    protected override BaseResult<BulkUpdateSplPayload>
        CreateValidationFailure(
            IReadOnlyList<ValidationError> errors) =>
        BaseResult<BulkUpdateSplPayload>.FromValidationErrors(
            errors);

    protected override async Task<
        BaseResult<BulkUpdateSplPayload>>
        ExecuteAsync(
            BulkUpdateSplCommand command,
            CancellationToken cancellationToken)
    {
        List<SplChangePreview> changes;

        if (!command.Preview &&
            command.ConfirmedChanges is not null)
        {
            changes = command.ConfirmedChanges.ToList();
        }
        else
        {
            var page =
                await _savedSearchRepository.GetPageAsync(
                    new SavedSearchQuery(
                        command.App,
                        command.Owner,
                        AlertsOnly: true),
                    cancellationToken);

            var searches = page.Items;

            var regex = new Regex(
                command.SplRegex,
                RegexOptions.None,
                TimeSpan.FromSeconds(2));

            var replacements =
                command.GroupReplacements is { Count: > 0 }
                    ? command.GroupReplacements
                    : ParseSingleReplacement(
                        command.Replacement);

            changes = searches
                .Where(search =>
                    regex.IsMatch(
                        search.SplSearchString))
                .Select(search =>
                    new SplChangePreview(
                        search.Id,
                        search.SplSearchString,
                        ReplaceNamedGroups(
                            regex,
                            search.SplSearchString,
                            replacements)))
                .Where(change =>
                    !string.Equals(
                        change.OriginalSpl,
                        change.UpdatedSpl,
                        StringComparison.Ordinal))
                .ToList();
        }

        if (command.Preview)
        {
            return BaseResult<BulkUpdateSplPayload>.Success(
                new BulkUpdateSplPayload(
                    0,
                    0,
                    changes));
        }

        var updates = changes
            .Select(change =>
                new SavedSearchUpdate(
                    change.Id,
                    change.UpdatedSpl))
            .ToList();

        var result =
            await _savedSearchRepository
                .UpdateSplStringsAsync(
                    updates,
                    cancellationToken);

        return BaseResult<BulkUpdateSplPayload>.Success(
            new BulkUpdateSplPayload(
                result.SucceededCount,
                result.FailedCount,
                changes));
    }

    private static IReadOnlyDictionary<string, string>
        ParseSingleReplacement(
            string replacement)
    {
        var separatorIndex =
            replacement.IndexOf('=');

        if (separatorIndex <= 0 ||
            separatorIndex == replacement.Length - 1)
        {
            throw new ArgumentException(
                "Replacement must use groupName=newValue format.",
                nameof(replacement));
        }

        return new Dictionary<string, string>(
            StringComparer.Ordinal)
        {
            [replacement[..separatorIndex].Trim()] =
                replacement[(separatorIndex + 1)..]
        };
    }

    private static string ReplaceNamedGroups(
        Regex regex,
        string input,
        IReadOnlyDictionary<string, string> replacements) =>
        regex.Replace(
            input,
            match =>
            {
                var result = match.Value;

                var edits = replacements
                    .Select(pair =>
                        (
                            Group: match.Groups[pair.Key],
                            pair.Value))
                    .Where(edit =>
                        edit.Group.Success)
                    .OrderByDescending(edit =>
                        edit.Group.Index)
                    .ToArray();

                foreach (var edit in edits)
                {
                    var start =
                        edit.Group.Index - match.Index;

                    result = string.Concat(
                        result.AsSpan(0, start),
                        edit.Value,
                        result.AsSpan(
                            start + edit.Group.Length));
                }

                return result;
            });
}