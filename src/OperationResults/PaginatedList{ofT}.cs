namespace OperationResults;

public class PaginatedList<T>
{
    public IEnumerable<T>? Items { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public bool HasNextPage { get; set; }

    public PaginatedList()
    {
    }

    public PaginatedList(IEnumerable<T>? items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
        HasNextPage = totalCount > (pageIndex * pageSize) + (items?.Count() ?? 0);
    }

    public PaginatedList(IEnumerable<T>? items, bool hasNextPage = false) : this(items, items?.Count() ?? 0, hasNextPage)
    {
    }

    public PaginatedList(IEnumerable<T>? items, int totalCount, bool hasNextPage = false)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = 0;
        PageSize = items?.Count() ?? 0;
        HasNextPage = items?.Count() != totalCount && hasNextPage;
    }
}
