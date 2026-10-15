namespace SplunkBulkUpdater.Application.Models.Paging;

public record PageResultInfo : PageQuery
{
    public bool IsLastPage { get; }

    public PageResultInfo(uint PageSize, uint PageNumber, bool isLastPage) : base(PageSize, PageNumber)
    {
        IsLastPage = isLastPage;
    }
}
