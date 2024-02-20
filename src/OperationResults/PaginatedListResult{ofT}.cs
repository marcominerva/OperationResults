﻿namespace OperationResults;

public class PaginatedListResult<T>
{
    public IEnumerable<T>? Items { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public bool HasNextPage { get; set; }

    public PaginatedListResult()
    {
    }

    public PaginatedListResult(IEnumerable<T>? items, int totalCount, int pageIndex, int pageSize = 0)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize > 0 ? pageSize : items?.Count() ?? 0;
        HasNextPage = totalCount > (pageIndex * PageSize) + (items?.Count() ?? 0);
    }

    public PaginatedListResult(IEnumerable<T>? items, bool hasNextPage = false) : this(items, items?.Count() ?? 0, hasNextPage)
    {
    }

    public PaginatedListResult(IEnumerable<T>? items, int totalCount, bool hasNextPage = false)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = 0;
        PageSize = items?.Count() ?? 0;
        HasNextPage = items?.Count() != totalCount && hasNextPage;
    }
}
