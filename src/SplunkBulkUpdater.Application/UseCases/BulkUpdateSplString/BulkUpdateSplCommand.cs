using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public sealed record BulkUpdateSplCommand(
    string SplRegex,
    string Replacement,
    string? App = null,
    string? Owner = null,
    bool Preview = false,
    IReadOnlyList<SplChangePreview>? ConfirmedChanges = null,
    IReadOnlyDictionary<string, string>? GroupReplacements = null)
    : ICommand<BaseResult<BulkUpdateSplPayload>>;
