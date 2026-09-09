using OperationResults;

namespace OperationResultsTests;

public class ResultMappingTests
{
    [Fact]
    public void Map_SuccessResult_MapsContent()
    {
        var mapped = Result<int>.Ok(42).Map(value => value.ToString());

        Assert.True(mapped.Success);
        Assert.Equal("42", mapped.Content);
    }

    [Fact]
    public void Map_NullContent_PassesNullToMapper()
    {
        string? received = "value";

        var mapped = Result<string?>.Ok(null).Map(value =>
        {
            received = value;
            return value?.Length ?? -1;
        });

        Assert.Null(received);
        Assert.Equal(-1, mapped.Content);
    }

    [Fact]
    public void Map_MapperReturnsNull_ReturnsSuccessfulNullContent()
    {
        var mapped = Result<int>.Ok(42).Map<int, string?>(_ => null);

        Assert.True(mapped.Success);
        Assert.Null(mapped.Content);
    }

    [Fact]
    public void Map_Failure_PreservesAllInformationAndDoesNotCallMapper()
    {
        var validationErrors = new[] { new ValidationError("Field", "Error") };
        var error = new InvalidOperationException("message", new Exception("detail"));
        var source = Result<int>.Fail(FailureReasons.DatabaseError, error, validationErrors);
        var mapperCalled = false;

        var mapped = source.Map(value =>
        {
            mapperCalled = true;
            return value.ToString();
        });

        Assert.False(mapperCalled);
        AssertFailureInformation(source, mapped);
    }

    [Fact]
    public void Map_FailureWithExplicitMessages_PreservesAllInformation()
    {
        var errors = new[] { new ValidationError("Field", "Error") };
        var source = Result<int>.Fail(FailureReasons.ClientError, "message", "detail", errors);

        var mapped = source.Map(value => value.ToString());

        AssertFailureInformation(source, mapped);
    }

    [Fact]
    public void Map_NullMapper_ThrowsBeforeInspectingResult()
    {
        var source = Result<int>.Fail(FailureReasons.ItemNotFound);

        var exception = Assert.Throws<ArgumentNullException>(() => source.Map<int, string>(null!));

        Assert.Equal("mapper", exception.ParamName);
    }

    [Fact]
    public void Map_MapperThrows_PropagatesSameException()
    {
        var error = new InvalidOperationException("mapping failed");

        var thrown = Assert.Throws<InvalidOperationException>(() => Result<int>.Ok(42).Map<int, string>(_ => throw error));

        Assert.Same(error, thrown);
    }

    [Fact]
    public void MapPaginated_Success_MapsEveryItemAndPreservesMetadata()
    {
        int[] items = [1, 2, 3];
        var source = Result<PaginatedList<int>>.Ok(new(items, 10, 2, 3, true));
        var received = new List<int>();

        var mapped = source.MapPaginated(value =>
        {
            received.Add(value);
            return value.ToString();
        });

        Assert.True(mapped.Success);
        Assert.NotNull(mapped.Content);
        Assert.Equal(["1", "2", "3"], mapped.Content.Items);
        Assert.Equal(items, received);
        Assert.Equal(10, mapped.Content.TotalCount);
        Assert.Equal(2, mapped.Content.PageIndex);
        Assert.Equal(3, mapped.Content.PageSize);
        Assert.True(mapped.Content.HasNextPage);
    }

    [Fact]
    public void MapPaginated_NullItems_PreservesNullAndDoesNotCallMapper()
    {
        var source = Result<PaginatedList<int>>.Ok(new(null, 10, 2, 3, true));
        var mapperCalled = false;

        var mapped = source.MapPaginated(value =>
        {
            mapperCalled = true;
            return value.ToString();
        });

        Assert.False(mapperCalled);
        Assert.NotNull(mapped.Content);
        Assert.Null(mapped.Content.Items);
        Assert.Equal(10, mapped.Content.TotalCount);
        Assert.Equal(2, mapped.Content.PageIndex);
        Assert.Equal(3, mapped.Content.PageSize);
        Assert.True(mapped.Content.HasNextPage);
    }

    [Fact]
    public void MapPaginated_EmptyItems_RemainsEmptyAndDoesNotCallMapper()
    {
        var source = Result<PaginatedList<int>>.Ok(new([], 0, 0, 25, false));
        var mapperCalled = false;

        var mapped = source.MapPaginated(value =>
        {
            mapperCalled = true;
            return value.ToString();
        });

        Assert.NotNull(mapped.Content);
        Assert.Empty(mapped.Content.Items!);
        Assert.False(mapperCalled);
        Assert.Equal(25, mapped.Content.PageSize);
    }

    [Fact]
    public void MapPaginated_Failure_PreservesAllInformationAndDoesNotCallMapper()
    {
        var errors = new[] { new ValidationError("Field", "Error") };
        var error = new InvalidOperationException("message", new Exception("detail"));
        var source = Result<PaginatedList<int>>.Fail(FailureReasons.DatabaseError, error, errors);
        var mapperCalled = false;

        var mapped = source.MapPaginated(value =>
        {
            mapperCalled = true;
            return value.ToString();
        });

        Assert.False(mapperCalled);
        AssertFailureInformation(source, mapped);
    }

    [Fact]
    public void MapPaginated_FailureWithExplicitMessages_PreservesAllInformation()
    {
        var errors = new[] { new ValidationError("Field", "Error") };
        var source = Result<PaginatedList<int>>.Fail(FailureReasons.ClientError, "message", "detail", errors);

        var mapped = source.MapPaginated(value => value.ToString());

        AssertFailureInformation(source, mapped);
    }

    [Fact]
    public void MapPaginated_NullMapper_ThrowsBeforeInspectingResult()
    {
        var source = Result<PaginatedList<int>>.Fail(FailureReasons.ItemNotFound);

        var exception = Assert.Throws<ArgumentNullException>(() => source.MapPaginated<int, string>(null!));

        Assert.Equal("mapper", exception.ParamName);
    }

    [Fact]
    public void MapPaginated_MapperThrows_PropagatesWhenItemsAreEnumerated()
    {
        var error = new InvalidOperationException("mapping failed");
        var mapped = Result<PaginatedList<int>>.Ok(new([42])).MapPaginated<int, string>(_ => throw error);

        var thrown = Assert.Throws<InvalidOperationException>(() => mapped.Content!.Items!.ToList());

        Assert.Same(error, thrown);
    }

    [Fact]
    public void MapPaginated_NullContent_ThrowsNullReferenceException()
    {
        var source = Result<PaginatedList<int>>.Ok(null);

        Assert.Throws<NullReferenceException>(() => source.MapPaginated(value => value.ToString()));
    }

    private static void AssertFailureInformation<TSource, TDestination>(Result<TSource> source, Result<TDestination> mapped)
    {
        Assert.False(mapped.Success);
        Assert.Null(mapped.Content);
        Assert.Equal(source.FailureReason, mapped.FailureReason);
        Assert.Equal(source.ErrorMessage, mapped.ErrorMessage);
        Assert.Equal(source.ErrorDetail, mapped.ErrorDetail);
        Assert.Same(source.Error, mapped.Error);
        Assert.Same(source.ValidationErrors, mapped.ValidationErrors);
        Assert.Equal(source.HasError, mapped.HasError);
    }
}