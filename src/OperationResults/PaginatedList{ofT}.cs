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
    /// Initializes the first page using the supplied items, when the total number of available items is unknown
    /// (for example, with continuation-token based paging).
    /// </summary>
    /// <param name="items">The items included in the page. <see langword="null"/> is treated as an empty page.</param>
    /// <param name="hasNextPage">A value indicating whether more items are available after this page.</param>
    /// <remarks>
    /// <see cref="TotalCount"/> and <see cref="PageSize"/> are set to the number of supplied items, so when
    /// <paramref name="hasNextPage"/> is <see langword="true"/>, <see cref="TotalCount"/> only reflects the current page.
    /// </remarks>
    public PaginatedList(IEnumerable<T>? items, bool hasNextPage = false)
    {
        var itemCount = CountItems(items);

        Items = items;
        TotalCount = itemCount;
        PageSize = itemCount;
        HasNextPage = hasNextPage;
    }

    /// <summary>
    /// Initializes the first page when the total number of available items is known.
    /// </summary>
    /// <param name="items">The items included in the page. <see langword="null"/> is treated as an empty page.</param>
    /// <param name="totalCount">The total number of items available across all pages.</param>
    /// <remarks>
    /// <see cref="PageSize"/> is set to the number of supplied items, and <see cref="HasNextPage"/> is <see langword="true"/>
    /// when <paramref name="totalCount"/> is greater than that number.
    /// </remarks>
    public PaginatedList(IEnumerable<T>? items, int totalCount)
    {
        var itemCount = CountItems(items);

        Items = items;
        TotalCount = totalCount;
        PageSize = itemCount;
        HasNextPage = HasMoreItems(totalCount, 0, itemCount, itemCount);
    }

    /// <summary>
    /// Initializes the first page when the total number of items and continuation state are known.
    /// </summary>
    /// <param name="items">The items included in the page. <see langword="null"/> is treated as an empty page.</param>
    /// <param name="totalCount">The total number of items available across all pages.</param>
    /// <param name="hasNextPage">A value indicating whether more items are available after this page.</param>
    public PaginatedList(IEnumerable<T>? items, int totalCount, bool hasNextPage)
        : this(items, totalCount, 0, CountItems(items), hasNextPage)
    {
    }

    /// <summary>
    /// Initializes a page and infers whether another page exists from the total count, page index, and page size.
    /// </summary>
    /// <param name="items">The items included in the page. <see langword="null"/> is treated as an empty page.</param>
    /// <param name="totalCount">The total number of items available across all pages.</param>
    /// <param name="pageIndex">The zero-based index of the current page.</param>
    /// <param name="pageSize">The requested page size, used to compute how many items precede the current page.</param>
    /// <remarks>
    /// <see cref="HasNextPage"/> is <see langword="true"/> when <paramref name="totalCount"/> is greater than the number of items
    /// in the previous pages (<paramref name="pageIndex"/> * <paramref name="pageSize"/>) plus the items of the current page.
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// // 7 items split in pages of 3: the third page (index 2) contains a single item and is the last one.
    /// var page = new PaginatedList&lt;Person&gt;(people, totalCount: 7, pageIndex: 2, pageSize: 3);
    /// // page.HasNextPage is false.
    /// </code>
    /// </example>
    public PaginatedList(IEnumerable<T>? items, int totalCount, int pageIndex, int pageSize)
        : this(items, totalCount, pageIndex, pageSize, HasMoreItems(totalCount, pageIndex, pageSize, CountItems(items)))
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

    private static int CountItems(IEnumerable<T>? items) => items?.Count() ?? 0;

    // Uses long arithmetic because pageIndex and pageSize may come from user input and their product can overflow.
    private static bool HasMoreItems(int totalCount, int pageIndex, int pageSize, int itemCount)
        => totalCount > ((long)pageIndex * pageSize) + itemCount;
}
