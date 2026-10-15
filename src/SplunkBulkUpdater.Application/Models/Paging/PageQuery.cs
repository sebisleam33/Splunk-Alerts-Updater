namespace SplunkBulkUpdater.Application.Models.Paging;

public record PageQuery(
    uint PageSize,
    uint PageNumber)
{
    public uint Offset => PageSize * PageNumber;
}


