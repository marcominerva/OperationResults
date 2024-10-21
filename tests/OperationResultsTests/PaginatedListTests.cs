using OperationResults;

namespace OperationResultsTests;

public class OperationResultsTests
{
    [Fact]
    public void PaginatedList_DefaultConstructor_ItemsIsNull()
    {
        // Arrange
        var paginatedList = new PaginatedList<int>();

        // Act

        // Assert
        Assert.Null(paginatedList.Items);
    }

    [Fact]
    public void PaginatedList_DefaultConstructor_PageIndexIsZero()
    {
        // Arrange
        var paginatedList = new PaginatedList<int>();

        // Act

        // Assert
        Assert.Equal(0, paginatedList.PageIndex);
    }

    [Fact]
    public void PaginatedList_DefaultConstructor_PageSizeIsZero()
    {
        // Arrange
        var paginatedList = new PaginatedList<int>();

        // Act

        // Assert
        Assert.Equal(0, paginatedList.PageSize);
    }

    [Fact]
    public void PaginatedList_DefaultConstructor_TotalCountIsZero()
    {
        // Arrange
        var paginatedList = new PaginatedList<int>();

        // Act

        // Assert
        Assert.Equal(0, paginatedList.TotalCount);
    }

    [Fact]
    public void PaginatedList_DefaultConstructor_HasNextPageIsFalse()
    {
        // Arrange
        var paginatedList = new PaginatedList<int>();

        // Act

        // Assert
        Assert.False(paginatedList.HasNextPage);
    }

    [Fact]
    public void PaginatedList_ItemsConstructor_ItemsSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items);

        // Act

        // Assert
        Assert.Equal(items, paginatedList.Items);
    }

    [Fact]
    public void PaginatedList_ItemsConstructor_PageIndexIsZero()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items);

        // Act

        // Assert
        Assert.Equal(0, paginatedList.PageIndex);
    }

    [Fact]
    public void PaginatedList_ItemsConstructor_PageSizeIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.PageSize);
    }

    [Fact]
    public void PaginatedList_ItemsConstructor_TotalCountIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.TotalCount);
    }

    [Fact]
    public void PaginatedList_ItemsConstructor_HasNextPageIsFalse()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items);

        // Act

        // Assert
        Assert.False(paginatedList.HasNextPage);
    }

    [Fact]
    public void PaginatedList_ItemsAndHasNextPageConstructor_ItemsSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, true);

        // Act

        // Assert
        Assert.Equal(items, paginatedList.Items);
    }

    [Fact]
    public void PaginatedList_ItemsAndHasNextPageConstructor_PageIndexIsZero()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, true);

        // Act

        // Assert
        Assert.Equal(0, paginatedList.PageIndex);
    }

    [Fact]
    public void PaginatedList_ItemsAndHasNextPageConstructor_PageSizeIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, true);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.PageSize);
    }

    [Fact]
    public void PaginatedList_ItemsAndHasNextPageConstructor_TotalCountIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, true);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.TotalCount);
    }

    [Fact]
    public void PaginatedList_ItemsAndHasNextPageConstructor_HasNextPageIsTrue()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, true);

        // Act

        // Assert
        Assert.True(paginatedList.HasNextPage);
    }

    [Fact]
    public void PaginatedList_ItemsAndTotalCountConstructor_ItemsSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count);

        // Act

        // Assert
        Assert.Equal(items, paginatedList.Items);
    }

    [Fact]
    public void PaginatedList_ItemsAndTotalCountConstructor_PageIndexIsZero()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count);

        // Act

        // Assert
        Assert.Equal(0, paginatedList.PageIndex);
    }

    [Fact]
    public void PaginatedList_ItemsAndTotalCountConstructor_PageSizeIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.PageSize);
    }

    [Fact]
    public void PaginatedList_ItemsAndTotalCountConstructor_TotalCountIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.TotalCount);
    }

    [Fact]
    public void PaginatedList_ItemsAndTotalCountConstructor_HasNextPageIsFalse()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count);

        // Act

        // Assert
        Assert.False(paginatedList.HasNextPage);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountAndHasNextPageConstructor_ItemsSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, true);

        // Act

        // Assert
        Assert.Equal(items, paginatedList.Items);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountAndHasNextPageConstructor_PageIndexIsZero()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, true);

        // Act

        // Assert
        Assert.Equal(0, paginatedList.PageIndex);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountAndHasNextPageConstructor_PageSizeIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, true);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.PageSize);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountAndHasNextPageConstructor_TotalCountIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, true);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.TotalCount);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountAndHasNextPageConstructor_HasNextPageIsTrue()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, true);

        // Act

        // Assert
        Assert.True(paginatedList.HasNextPage);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexConstructor_ItemsSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1);

        // Act

        // Assert
        Assert.Equal(items, paginatedList.Items);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexConstructor_PageIndexSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageIndex = 1;
        var paginatedList = new PaginatedList<int>(items, items.Count, pageIndex);

        // Act

        // Assert
        Assert.Equal(pageIndex, paginatedList.PageIndex);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexConstructor_PageSizeIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.PageSize);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexConstructor_TotalCountIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.TotalCount);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexConstructor_HasNextPageIsFalse()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1);

        // Act

        // Assert
        Assert.False(paginatedList.HasNextPage);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeConstructor_ItemsSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1, 10);

        // Act

        // Assert
        Assert.Equal(items, paginatedList.Items);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeConstructor_PageIndexSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageIndex = 1;
        var paginatedList = new PaginatedList<int>(items, items.Count, pageIndex, 10);

        // Act

        // Assert
        Assert.Equal(pageIndex, paginatedList.PageIndex);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeConstructor_PageSizeSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageSize = 10;
        var paginatedList = new PaginatedList<int>(items, items.Count, 1, pageSize);

        // Act

        // Assert
        Assert.Equal(pageSize, paginatedList.PageSize);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeConstructor_TotalCountIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1, 10);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.TotalCount);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeConstructor_HasNextPageIsFalse()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1, 10);

        // Act

        // Assert
        Assert.False(paginatedList.HasNextPage);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeHasNextPageConstructor_ItemsSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1, 10, true);

        // Act

        // Assert
        Assert.Equal(items, paginatedList.Items);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeHasNextPageConstructor_PageIndexSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageIndex = 1;
        var paginatedList = new PaginatedList<int>(items, items.Count, pageIndex, 10, true);

        // Act

        // Assert
        Assert.Equal(pageIndex, paginatedList.PageIndex);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeHasNextPageConstructor_PageSizeSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageSize = 10;
        var paginatedList = new PaginatedList<int>(items, items.Count, 1, pageSize, true);

        // Act

        // Assert
        Assert.Equal(pageSize, paginatedList.PageSize);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeHasNextPageConstructor_TotalCountIsItemCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, items.Count, 1, 10, true);

        // Act

        // Assert
        Assert.Equal(items.Count, paginatedList.TotalCount);
    }

    [Fact]
    public void PaginatedList_ItemsTotalCountPageIndexPageSizeHasNextPageConstructor_HasNextPageSetCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var hasNextPage = true;
        var paginatedList = new PaginatedList<int>(items, items.Count, 1, 10, hasNextPage);

        // Act

        // Assert
        Assert.Equal(hasNextPage, paginatedList.HasNextPage);
    }
}