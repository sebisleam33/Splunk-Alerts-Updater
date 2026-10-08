using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public record GetAlertsCommand(
    string SplRegex,
    string? App = null,
    string? Owner = null
    ) : ICommand<BaseResult<IReadOnlyList<GetAlertsPayload>>>;
