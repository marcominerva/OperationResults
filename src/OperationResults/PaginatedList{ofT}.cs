namespace OperationResults;

/// <summary>
/// Represents a page of items together with the metadata needed by adapters and clients to continue browsing a larger result set.
/// </summary>
/// <typeparam name="T">The type of item contained in the page.</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Gets or sets the items included in the current page.
    /// </summary>
    public IEnumerable<T>? Items { get; set; }

    /// <summary>
    /// Gets or sets the zero-based index of the current page.
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// Gets or sets the requested or returned page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items available across all pages when that value is known.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether another page can be requested after the current one.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Initializes an empty pagination container for serializers, mappers, and frameworks that require a parameterless constructor.
    /// </summary>
    public PaginatedList()
    {
    }

    /// <summary>
    /// Initializes a page using the supplied items and infers the total count from the current page.
    /// </summary>
    /// <param name="items">The items included in the page.</param>
    /// <param name="hasNextPage">A value indicating whether more items are available after this page.</param>
    public PaginatedList(IEnumerable<T>? items, bool hasNextPage = false)
        : this(items, items?.Count() ?? 0, hasNextPage)
    {
    }

    /// <summary>
    /// Initializes a page when the total number of available items is known.
    /// </summary>
    /// <param name="items">The items included in the page.</param>
    /// <param name="totalCount">The total number of items available across all pages.</param>
    public PaginatedList(IEnumerable<T>? items, int totalCount)
        : this(items, totalCount, totalCount > items?.Count())
    {
    }

    /// <summary>
    /// Initializes a page when the total number of items and continuation state are known.
    /// </summary>
    /// <param name="items">The items included in the page.</param>
    /// <param name="totalCount">The total number of items available across all pages.</param>
    /// <param name="hasNextPage">A value indicating whether more items are available after this page.</param>
    public PaginatedList(IEnumerable<T>? items, int totalCount, bool hasNextPage)
        : this(items, totalCount, 0, items?.Count() ?? 0, hasNextPage)
    {
    }

    /// <summary>
    /// Initializes a page for a known page index while inferring the page size from the supplied items.
    /// </summary>
    /// <param name="items">The items included in the page.</param>
    /// <param name="totalCount">The total number of items available across all pages.</param>
    /// <param name="pageIndex">The zero-based index of the current page.</param>
    public PaginatedList(IEnumerable<T>? items, int totalCount, int pageIndex)
        : this(items, totalCount, pageIndex, items?.Count() ?? 0)
    {
    }

    /// <summary>
    /// Initializes a page and infers whether another page exists from the total count, page index, and page size.
    /// </summary>
    /// <param name="items">The items included in the page.</param>
    /// <param name="totalCount">The total number of items available across all pages.</param>
    /// <param name="pageIndex">The zero-based index of the current page.</param>
    /// <param name="pageSize">The requested or returned page size.</param>
    public PaginatedList(IEnumerable<T>? items, int totalCount, int pageIndex, int pageSize)
        : this(items, totalCount, pageIndex, pageSize, totalCount > (pageIndex * pageSize) + (items?.Count() ?? 0))
    {
    }

    /// <summary>
    /// Initializes a page with explicit item, count, position, size, and continuation metadata.
    /// </summary>
    /// <param name="items">The items included in the page.</param>
    /// <param name="totalCount">The total number of items available across all pages.</param>
    /// <param name="pageIndex">The zero-based index of the current page.</param>
    /// <param name="pageSize">The requested or returned page size.</param>
    /// <param name="hasNextPage">A value indicating whether more items are available after this page.</param>
    public PaginatedList(IEnumerable<T>? items, int totalCount, int pageIndex, int pageSize, bool hasNextPage)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
        HasNextPage = hasNextPage;
    }
}
