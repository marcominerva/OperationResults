using OperationResults;

namespace OperationResultsTests;

public class PaginatedListTests
{
    [Fact]
    public void DefaultConstructor_UsesDefaultValues()
    {
        var result = new PaginatedList<int>();

        Assert.Null(result.Items);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(0, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.False(result.HasNextPage);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ItemsConstructor_InfersCountsAndPreservesContinuation(bool hasNextPage)
    {
        int[] items = [1, 2, 3];

        var result = new PaginatedList<int>(items, hasNextPage);

        Assert.Same(items, result.Items);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(hasNextPage, result.HasNextPage);
    }

    [Fact]
    public void ItemsConstructor_WithNullItems_UsesZeroCounts()
    {
        var result = new PaginatedList<int>(null);

        Assert.Null(result.Items);
        Assert.Equal(0, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void ItemsConstructor_WithEmptyItems_PreservesEmptyCollection()
    {
        int[] items = [];

        var result = new PaginatedList<int>(items);

        Assert.Same(items, result.Items);
        Assert.Equal(0, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.False(result.HasNextPage);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(3, false)]
    [InlineData(2, false)]
    public void ItemsAndTotalCountConstructor_InfersContinuation(int totalCount, bool expectedHasNextPage)
    {
        int[] items = [1, 2, 3];

        var result = new PaginatedList<int>(items, totalCount);

        Assert.Same(items, result.Items);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(expectedHasNextPage, result.HasNextPage);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(0, false)]
    public void ItemsAndTotalCountConstructor_WithNullItems_BehavesLikeEmptyItems(int totalCount, bool expectedHasNextPage)
    {
        var nullItemsResult = new PaginatedList<int>(null, totalCount);
        var emptyItemsResult = new PaginatedList<int>([], totalCount);

        Assert.Null(nullItemsResult.Items);
        Assert.Equal(0, nullItemsResult.PageIndex);
        Assert.Equal(0, nullItemsResult.PageSize);
        Assert.Equal(totalCount, nullItemsResult.TotalCount);
        Assert.Equal(expectedHasNextPage, nullItemsResult.HasNextPage);
        Assert.Equal(emptyItemsResult.HasNextPage, nullItemsResult.HasNextPage);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ItemsTotalCountAndHasNextPageConstructor_PreservesMetadata(bool hasNextPage)
    {
        int[] items = [1, 2];

        var result = new PaginatedList<int>(items, 10, hasNextPage);

        Assert.Same(items, result.Items);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(hasNextPage, result.HasNextPage);
    }

    [Theory]
    [InlineData(7, 0, 3, 3, true)]
    [InlineData(7, 1, 3, 3, true)]
    [InlineData(7, 2, 3, 1, false)]
    [InlineData(9, 2, 3, 3, false)]
    [InlineData(10, 2, 3, 3, true)]
    [InlineData(3, 0, 3, 3, false)]
    [InlineData(0, 0, 10, 0, false)]
    [InlineData(7, 5, 3, 0, false)]
    [InlineData(11, 1, 5, 3, true)]
    [InlineData(8, 1, 5, 3, false)]
    [InlineData(10, 1, 5, 0, true)]
    public void ItemsTotalCountPageIndexAndPageSizeConstructor_InfersContinuation(int totalCount, int pageIndex, int pageSize, int itemCount, bool expectedHasNextPage)
    {
        var items = Enumerable.Range(1, itemCount).ToArray();

        var result = new PaginatedList<int>(items, totalCount, pageIndex, pageSize);

        Assert.Same(items, result.Items);
        Assert.Equal(pageIndex, result.PageIndex);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(expectedHasNextPage, result.HasNextPage);
    }

    [Fact]
    public void ItemsTotalCountPageIndexAndPageSizeConstructor_WithNullItems_InfersContinuation()
    {
        var result = new PaginatedList<int>(null, 11, 1, 5);

        Assert.Null(result.Items);
        Assert.Equal(1, result.PageIndex);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(11, result.TotalCount);
        Assert.True(result.HasNextPage);
    }

    [Theory]
    [InlineData(1_073_741_824, 4)]
    [InlineData(int.MaxValue, 2)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void ItemsTotalCountPageIndexAndPageSizeConstructor_WithLargePageIndexAndSize_DoesNotOverflow(int pageIndex, int pageSize)
    {
        var result = new PaginatedList<int>([1], 100, pageIndex, pageSize);

        Assert.Equal(pageIndex, result.PageIndex);
        Assert.Equal(pageSize, result.PageSize);
        Assert.False(result.HasNextPage);
    }

    public enum InferringConstructor
    {
        Items,
        ItemsAndHasNextPage,
        ItemsAndTotalCount,
        ItemsTotalCountAndHasNextPage,
        ItemsTotalCountPageIndexAndPageSize
    }

    [Theory]
    [InlineData(InferringConstructor.Items)]
    [InlineData(InferringConstructor.ItemsAndHasNextPage)]
    [InlineData(InferringConstructor.ItemsAndTotalCount)]
    [InlineData(InferringConstructor.ItemsTotalCountAndHasNextPage)]
    [InlineData(InferringConstructor.ItemsTotalCountPageIndexAndPageSize)]
    public void InferringConstructors_EnumerateItemsAtMostOnce(InferringConstructor constructor)
    {
        var items = new CountingEnumerable([1, 2, 3]);

        _ = constructor switch
        {
            InferringConstructor.Items => new PaginatedList<int>(items),
            InferringConstructor.ItemsAndHasNextPage => new PaginatedList<int>(items, true),
            InferringConstructor.ItemsAndTotalCount => new PaginatedList<int>(items, 10),
            InferringConstructor.ItemsTotalCountAndHasNextPage => new PaginatedList<int>(items, 10, true),
            InferringConstructor.ItemsTotalCountPageIndexAndPageSize => new PaginatedList<int>(items, 10, 1, 3),
            _ => throw new ArgumentOutOfRangeException(nameof(constructor))
        };

        Assert.Equal(1, items.EnumerationCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullConstructor_PreservesAllValues(bool hasNextPage)
    {
        int[] items = [1, 2];

        var result = new PaginatedList<int>(items, 25, 2, 10, hasNextPage);

        Assert.Same(items, result.Items);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(hasNextPage, result.HasNextPage);
    }

    private sealed class CountingEnumerable(IEnumerable<int> items) : IEnumerable<int>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<int> GetEnumerator()
        {
            EnumerationCount++;
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}