using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public record BulkUpdateSplCommand(
    string SplRegex,
    string Replacement,
    string? App = null,
    string? Owner = null) : ICommand<BaseResult<BulkUpdateSplPayload>>;
