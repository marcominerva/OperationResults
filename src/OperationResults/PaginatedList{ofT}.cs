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

    public PaginatedList(IEnumerable<T>? items, bool hasNextPage = false)
        : this(items, items?.Count() ?? 0, hasNextPage)
    {
    }

    public PaginatedList(IEnumerable<T>? items, int totalCount)
        : this(items, totalCount, items?.Count() != totalCount)
    {
    }

    public PaginatedList(IEnumerable<T>? items, int totalCount, bool hasNextPage)
        : this(items, totalCount, 0, items?.Count() ?? 0, hasNextPage)
    {
    }

    public PaginatedList(IEnumerable<T>? items, int totalCount, int pageIndex)
        : this(items, totalCount, pageIndex, items?.Count() ?? 0)
    {
    }

    public PaginatedList(IEnumerable<T>? items, int totalCount, int pageIndex, int pageSize)
        : this(items, totalCount, pageIndex, pageSize, totalCount > (pageIndex * pageSize) + (items?.Count() ?? 0))
    {
    }

    public PaginatedList(IEnumerable<T>? items, int totalCount, int pageIndex, int pageSize, bool hasNextPage)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
        HasNextPage = hasNextPage;
    }
}
