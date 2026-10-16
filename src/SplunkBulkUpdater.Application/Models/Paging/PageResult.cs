namespace SplunkBulkUpdater.Application.Models.Paging;

public record PageResult<T>(
    IReadOnlyList<T> Items,
    PageResultInfo Page);