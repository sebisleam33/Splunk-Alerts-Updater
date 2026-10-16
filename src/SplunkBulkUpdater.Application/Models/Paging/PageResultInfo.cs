namespace SplunkBulkUpdater.Application.Models.Paging;

public sealed record PageResultInfo : PageQuery
{
    public int ReturnedCount { get; }
    public long TotalCount { get; }

    public bool IsLastPage =>
        (long)Offset + ReturnedCount >= TotalCount;

    public PageResultInfo(
        int pageSize,
        int pageNumber,
        int returnedCount,
        long totalCount)
        : base(pageSize, pageNumber)
    {
        if (returnedCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(returnedCount));
        }

        if (totalCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCount));
        }

        ReturnedCount = returnedCount;
        TotalCount = totalCount;
    }
}