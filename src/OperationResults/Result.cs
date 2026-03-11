using System.Diagnostics.CodeAnalysis;

namespace OperationResults;

public class Result : IGenericResult
{
    public bool Success { get; }

    public int FailureReason { get; }

    public Exception? Error { get; }

    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool HasError => Error is not null;

    private readonly string? errorMessage;
    public string? ErrorMessage => errorMessage ?? Error?.Message;

    private readonly string? errorDetail;
    public string? ErrorDetail => errorDetail ?? Error?.InnerException?.Message;

    public IEnumerable<ValidationError>? ValidationErrors { get; }

    internal Result(bool success = true, int failureReason = FailureReasons.None, string? message = null, string? detail = null, Exception? error = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        Success = success;
        FailureReason = failureReason;
        errorMessage = message;
        errorDetail = detail;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static Result Ok()
        => new(success: true);

    public static Result Fail(int failureReason, ValidationError validationError)
        => new(false, failureReason: failureReason, validationErrors: [validationError]);

    public static Result Fail(int failureReason, string? message, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, validationErrors: [validationError]);

    public static Result Fail(int failureReason, string? message, string? detail, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: [validationError]);

    public static Result Fail(int failureReason, Exception? error, ValidationError validationError)
        => new(false, failureReason: failureReason, error: error, validationErrors: [validationError]);

    public static Result Fail(int failureReason, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, validationErrors: validationErrors);

    public static Result Fail(int failureReason, string? message, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, validationErrors: validationErrors);

    public static Result Fail(int failureReason, string? message, string? detail, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: validationErrors);

    public static Result Fail(int failureReason, Exception? error, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, error: error, validationErrors: validationErrors);

    /// <summary>
    /// Maps the content of a successful <see cref="Result{T}"/> to a new <see cref="Result{TDestination}"/> using the specified <paramref name="mapper"/> function.
    /// If the source result is a failure, a new failure result with the same error information is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source result content.</typeparam>
    /// <typeparam name="TDestination">The type of the destination result content.</typeparam>
    /// <param name="source">The source result to map.</param>
    /// <param name="mapper">A function to transform the source content to the destination type.</param>
    /// <returns>A new <see cref="Result{TDestination}"/> with the mapped content or the original error information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    public static Result<TDestination> Map<TSource, TDestination>(Result<TSource> source, Func<TSource, TDestination> mapper)
        => source.MapContent(mapper);

    /// <summary>
    /// Maps the items of a successful <see cref="Result{T}"/> containing a <see cref="PaginatedList{TSource}"/> to a new
    /// <see cref="Result{T}"/> containing a <see cref="PaginatedList{TDestination}"/> using the specified <paramref name="mapper"/> function.
    /// If the source result is a failure, a new failure result with the same error information is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source paginated list items.</typeparam>
    /// <typeparam name="TDestination">The type of the destination paginated list items.</typeparam>
    /// <param name="source">The source result containing a <see cref="PaginatedList{TSource}"/> to map.</param>
    /// <param name="mapper">A function to transform each item from the source type to the destination type.</param>
    /// <returns>A new <see cref="Result{T}"/> of <see cref="PaginatedList{TDestination}"/> with the mapped items or the original error information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    public static Result<PaginatedList<TDestination>> MapPaginated<TSource, TDestination>(Result<PaginatedList<TSource>> source, Func<TSource, TDestination> mapper)
        => source.MapPaginatedContent(mapper);

    public static bool operator true(Result result)
        => result.Success;

    public static bool operator false(Result result)
        => !result.Success;

    public static implicit operator bool(Result result)
        => result.Success;
}
