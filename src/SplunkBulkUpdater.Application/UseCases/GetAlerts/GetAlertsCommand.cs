using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.GetAlerts;

public record GetAlertsCommand(
    string SplRegex,
    string? App = null,
    string? Owner = null,
    ChangeType? ChangeType = null,
    PageQuery? Paging = null)
    : ICommand<
        BaseResult<
            PageResult<GetAlertsPayload>>>;