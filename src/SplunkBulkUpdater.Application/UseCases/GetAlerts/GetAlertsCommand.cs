using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public record GetAlertsCommand(
    string SplRegex,
    string? App = null,
    string? Owner = null,
    ChangeType? ChangeType = null
    ) : ICommand<BaseResult<IReadOnlyList<GetAlertsPayload>>>;
