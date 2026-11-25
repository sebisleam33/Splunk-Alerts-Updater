using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.GetAlerts;

public record GetAlertsCommand(
    string App,
    string Owner,
    PageQuery Paging,
    ChangeType? ChangeType = null,
    IEnumerable<KeyValuePair<string, string>>? SearchStringFilters = null)
    : ICommand<
        BaseResult<
            PageResult<GetAlertsPayload>>>;