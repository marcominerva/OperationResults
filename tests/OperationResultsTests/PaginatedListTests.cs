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

    [Fact]
    public void ItemsAndTotalCountConstructor_WithNullItems_DoesNotInferContinuation()
    {
        var result = new PaginatedList<int>(null, 10);

        Assert.Null(result.Items);
        Assert.Equal(0, result.PageSize);
        Assert.Equal(10, result.TotalCount);
        Assert.False(result.HasNextPage);
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
    [InlineData(7, 1, true)]
    [InlineData(6, 1, false)]
    [InlineData(3, 0, false)]
    public void ItemsTotalCountAndPageIndexConstructor_InfersMetadata(int totalCount, int pageIndex, bool expectedHasNextPage)
    {
        int[] items = [1, 2, 3];

        var result = new PaginatedList<int>(items, totalCount, pageIndex);

        Assert.Same(items, result.Items);
        Assert.Equal(pageIndex, result.PageIndex);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(expectedHasNextPage, result.HasNextPage);
    }

    [Fact]
    public void ItemsTotalCountAndPageIndexConstructor_WithNullItems_UsesZeroPageSize()
    {
        var result = new PaginatedList<int>(null, 10, 2);

        Assert.Null(result.Items);
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(0, result.PageSize);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
    }

    [Theory]
    [InlineData(11, 1, 5, 3, true)]
    [InlineData(8, 1, 5, 3, false)]
    [InlineData(10, 1, 5, 0, true)]
    public void ItemsTotalCountPageIndexAndPageSizeConstructor_InfersContinuation(
        int totalCount, int pageIndex, int pageSize, int itemCount, bool expectedHasNextPage)
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
}