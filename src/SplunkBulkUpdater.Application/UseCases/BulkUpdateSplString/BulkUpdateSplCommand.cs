using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public sealed record BulkUpdateSplCommand(
    string App,
    string Owner,
    ReplacementPair ReplacementPair,
    IReadOnlySet<SavedSearchId> SelectedSearches,
    bool Preview = false,
    IReadOnlyList<SplChangePreview>? ConfirmedChanges = default,
    IReadOnlyDictionary<string, string>? SearchStringFilters = null)
    : ICommand<BaseResult<BulkUpdateSplPayload>>;
