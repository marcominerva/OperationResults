namespace OperationResults;

/// <summary>
/// Provides extension methods for mapping <see cref="Result{T}"/> content to a different type.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps the content of a successful <see cref="Result{TSource}"/> to a new <see cref="Result{TDestination}"/> using the specified <paramref name="mapper"/> function.
    /// If the source result is a failure, a new failure result with the same error information is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source result content.</typeparam>
    /// <typeparam name="TDestination">The type of the destination result content.</typeparam>
    /// <param name="source">The source result to map.</param>
    /// <param name="mapper">A function to transform the source content to the destination type.</param>
    /// <returns>A new <see cref="Result{TDestination}"/> with the mapped content or the original error information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    public static Result<TDestination> Map<TSource, TDestination>(this Result<TSource> source, Func<TSource, TDestination> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        Result<TDestination> result = source switch
        {
            TSource value => mapper(value),
            Error error => new Error(error.FailureReason, error.ErrorMessage, error.ErrorDetail, error.Exception, error.ValidationErrors),
            null => throw new InvalidOperationException("Unexpected result type.")
        };

        return result;
    }

    /// <summary>
    /// Maps the items of a successful <see cref="Result{T}"/> containing a <see cref="PaginatedList{TSource}"/> to a new
    /// <see cref="Result{T}"/> containing a <see cref="PaginatedList{TDestination}"/> using the specified <paramref name="mapper"/> function.
    /// Pagination metadata (TotalCount, PageIndex, PageSize, HasNextPage) is preserved.
    /// If the source result is a failure, a new failure result with the same error information is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source paginated list items.</typeparam>
    /// <typeparam name="TDestination">The type of the destination paginated list items.</typeparam>
    /// <param name="source">The source result containing a <see cref="PaginatedList{TSource}"/> to map.</param>
    /// <param name="mapper">A function to transform each item from the source type to the destination type.</param>
    /// <returns>A new <see cref="Result{T}"/> of <see cref="PaginatedList{TDestination}"/> with the mapped items or the original error information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    public static Result<PaginatedList<TDestination>> MapPaginated<TSource, TDestination>(this Result<PaginatedList<TSource>> source, Func<TSource, TDestination> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        Result<PaginatedList<TDestination>> result = source switch
        {
            PaginatedList<TSource> content => new PaginatedList<TDestination>(content.Items?.Select(mapper), content.TotalCount, content.PageIndex, content.PageSize, content.HasNextPage),
            Error error => new Error(error.FailureReason, error.ErrorMessage, error.ErrorDetail, error.Exception, error.ValidationErrors),
            null => throw new InvalidOperationException("Unexpected result type.")
        };

        return result;
    }
}
