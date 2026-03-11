using OperationResults;

namespace OperationResultsTests;

public class ResultMappingTests
{
    [Fact]
    public void MapContent_SuccessResult_MapsContent()
    {
        var source = Result<int>.Ok(42);

        var mapped = source.Map(x => x.ToString());

        Assert.True(mapped.Success);
        Assert.Equal("42", mapped.Content);
    }

    [Fact]
    public void MapContent_SuccessResultWithNullContent_MapsContent()
    {
        var source = Result<string?>.Ok(null);

        var mapped = source.Map(x => x?.Length ?? -1);

        Assert.Equal(-1, mapped.Content);
    }

    [Fact]
    public void MapContent_FailedResultWithError_PreservesFailureReason()
    {
        var error = new InvalidOperationException("test error");
        var source = Result<int>.Fail(FailureReasons.ItemNotFound, error);

        var mapped = source.Map(x => x.ToString());

        Assert.False(mapped.Success);
        Assert.Equal(FailureReasons.ItemNotFound, mapped.FailureReason);
    }

    [Fact]
    public void MapContent_FailedResultWithError_PreservesException()
    {
        var error = new InvalidOperationException("test error");
        var source = Result<int>.Fail(FailureReasons.ItemNotFound, error);

        var mapped = source.Map(x => x.ToString());

        Assert.Same(error, mapped.Error);
    }

    [Fact]
    public void MapContent_FailedResultWithMessage_PreservesErrorMessage()
    {
        var source = Result<int>.Fail(FailureReasons.ClientError, "error message", "error detail");

        var mapped = source.Map(x => x.ToString());

        Assert.False(mapped.Success);
        Assert.Equal("error message", mapped.ErrorMessage);
        Assert.Equal("error detail", mapped.ErrorDetail);
    }

    [Fact]
    public void MapContent_FailedResultWithValidationErrors_PreservesValidationErrors()
    {
        var validationErrors = new List<ValidationError>
        {
            new("Field1", "Error1"),
            new("Field2", "Error2")
        };
        var source = Result<int>.Fail(FailureReasons.InvalidRequest, "msg", validationErrors);

        var mapped = source.Map(x => x.ToString());

        Assert.False(mapped.Success);
        Assert.Equal(validationErrors, mapped.ValidationErrors);
    }

    [Fact]
    public void MapContent_FailedResult_DoesNotCallMapper()
    {
        var source = Result<int>.Fail(FailureReasons.ItemNotFound);
        var mapperCalled = false;

        var mapped = source.Map(x =>
        {
            mapperCalled = true;
            return x.ToString();
        });

        Assert.False(mapperCalled);
        Assert.False(mapped.Success);
    }

    [Fact]
    public void MapContent_NullMapper_ThrowsArgumentNullException()
    {
        var source = Result<int>.Ok(42);

        Assert.Throws<ArgumentNullException>(() => source.Map<int, string>(null!));
    }

    [Fact]
    public void MapPaginatedContent_SuccessResult_MapsItems()
    {
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, 10, 0, 3, true);
        var source = Result<PaginatedList<int>>.Ok(paginatedList);

        var mapped = source.MapPaginated(x => x.ToString());

        Assert.True(mapped.Success);
        Assert.NotNull(mapped.Content);
        Assert.Equal(["1", "2", "3"], mapped.Content.Items);
    }

    [Fact]
    public void MapPaginatedContent_SuccessResult_PreservesPaginationMetadata()
    {
        var items = new List<int> { 1, 2, 3 };
        var paginatedList = new PaginatedList<int>(items, 10, 2, 3, true);
        var source = Result<PaginatedList<int>>.Ok(paginatedList);

        var mapped = source.MapPaginated(x => x.ToString());

        Assert.True(mapped.Success);
        Assert.NotNull(mapped.Content);
        Assert.Equal(10, mapped.Content.TotalCount);
        Assert.Equal(2, mapped.Content.PageIndex);
        Assert.Equal(3, mapped.Content.PageSize);
        Assert.True(mapped.Content.HasNextPage);
    }

    [Fact]
    public void MapPaginatedContent_FailedResult_PreservesFailureInfo()
    {
        var error = new InvalidOperationException("test error");
        var validationErrors = new List<ValidationError> { new("Field", "Error") };
        var source = Result<PaginatedList<int>>.Fail(FailureReasons.DatabaseError, error, validationErrors);

        var mapped = source.MapPaginated(x => x.ToString());

        Assert.False(mapped.Success);
        Assert.Equal(FailureReasons.DatabaseError, mapped.FailureReason);
        Assert.Same(error, mapped.Error);
        Assert.Equal(validationErrors, mapped.ValidationErrors);
    }

    [Fact]
    public void MapPaginatedContent_FailedResultWithMessage_PreservesErrorMessage()
    {
        var source = Result<PaginatedList<int>>.Fail(FailureReasons.ClientError, "error message", "error detail");

        var mapped = source.MapPaginated(x => x.ToString());

        Assert.False(mapped.Success);
        Assert.Equal("error message", mapped.ErrorMessage);
        Assert.Equal("error detail", mapped.ErrorDetail);
    }

    [Fact]
    public void MapPaginatedContent_NullMapper_ThrowsArgumentNullException()
    {
        var source = Result<PaginatedList<int>>.Ok(new PaginatedList<int>());

        Assert.Throws<ArgumentNullException>(() => source.MapPaginated<int, string>(null!));
    }
}
