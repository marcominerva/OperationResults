namespace OperationResults;

/// <summary>
/// Provides extension methods for mapping <see cref="Result{T}"/> content to a different type.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps the content of a successful <see cref="Result{TSource}"/> to a new <see cref="Result{TDestination}"/> using the specified <paramref name="mapper"/> function.
    /// If the source result is a failure, a new failure result with the same <see cref="Error"/> is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source result content.</typeparam>
    /// <typeparam name="TDestination">The type of the destination result content.</typeparam>
    /// <param name="source">The source result to map.</param>
    /// <param name="mapper">A function to transform the source content to the destination type. It is also invoked when a successful result has <see langword="null"/> content.</param>
    /// <returns>A new <see cref="Result{TDestination}"/> with the mapped content or the original error information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is a <see langword="default"/> instance that was never initialized.</exception>
    public static Result<TDestination> Map<TSource, TDestination>(this Result<TSource> source, Func<TSource, TDestination> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (source.Success)
        {
            return mapper(source.Content!);
        }

        return GetError(source);
    }

    /// <summary>
    /// Maps the items of a successful <see cref="Result{T}"/> containing a <see cref="PaginatedList{TSource}"/> to a new
    /// <see cref="Result{T}"/> containing a <see cref="PaginatedList{TDestination}"/> using the specified <paramref name="mapper"/> function.
    /// Pagination metadata (TotalCount, PageIndex, PageSize, HasNextPage) is preserved.
    /// If the source result is a failure, a new failure result with the same <see cref="Error"/> is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source paginated list items.</typeparam>
    /// <typeparam name="TDestination">The type of the destination paginated list items.</typeparam>
    /// <param name="source">The source result containing a <see cref="PaginatedList{TSource}"/> to map.</param>
    /// <param name="mapper">A function to transform each item from the source type to the destination type.</param>
    /// <returns>
    /// A new <see cref="Result{T}"/> of <see cref="PaginatedList{TDestination}"/> with the mapped items or the original error information.
    /// A successful result with <see langword="null"/> content is mapped to a successful result with <see langword="null"/> content.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is a <see langword="default"/> instance that was never initialized.</exception>
    public static Result<PaginatedList<TDestination>> MapPaginated<TSource, TDestination>(this Result<PaginatedList<TSource>> source, Func<TSource, TDestination> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (source.Success)
        {
            return source.Content is { } content
                ? new PaginatedList<TDestination>(content.Items?.Select(mapper), content.TotalCount, content.PageIndex, content.PageSize, content.HasNextPage)
                : default(PaginatedList<TDestination>)!;
        }

        return GetError(source);
    }

    private static Error GetError<T>(Result<T> source)
        => source is Error error
            ? error
            : throw new InvalidOperationException($"Cannot map a default {nameof(Result<>)} instance that was never initialized with content or an error.");
}
