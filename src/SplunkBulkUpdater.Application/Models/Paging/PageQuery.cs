namespace SplunkBulkUpdater.Application.Models.Paging;

public record PageQuery
{
    public const int DefaultPageSize = 100;
    public const int MaxPageSize = 500;

    public int PageSize { get; }
    public int PageNumber { get; }

    public int Offset => checked(PageSize * PageNumber);

    public PageQuery(
        int pageSize = DefaultPageSize,
        int pageNumber = 0)
    {
        if (pageSize is < 1 or > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                $"Page size must be between 1 and {MaxPageSize}.");
        }

        if (pageNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        PageSize = pageSize;
        PageNumber = pageNumber;
    }
}