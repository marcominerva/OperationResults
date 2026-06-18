using System.Diagnostics.CodeAnalysis;

namespace OperationResults;

/// <summary>
/// Represents the outcome of an operation that can produce content when it succeeds.
/// </summary>
/// <typeparam name="T">The type of content returned by the operation.</typeparam>
/// <remarks>
/// Use this type in application and business services to return values together with failure metadata while remaining independent from protocol-specific response types.
/// This allows adapters, such as ASP.NET Core endpoints, to translate the same business result into the response format required by the host application.
/// </remarks>
public class Result<T> : IGenericResult<T>
{
    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Content))]
    public bool Success { get; }

    /// <inheritdoc />
    public T? Content { get; }

    /// <inheritdoc />
    public int FailureReason { get; }

    /// <inheritdoc />
    public Exception? Error { get; }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool HasError => Error is not null;

    /// <inheritdoc />
    public string? ErrorMessage => field ?? Error?.Message;

    /// <inheritdoc />
    public string? ErrorDetail => field ?? Error?.InnerException?.Message;

    /// <inheritdoc />
    public IEnumerable<ValidationError>? ValidationErrors { get; }

    internal Result(bool success = true, T? content = default, int failureReason = FailureReasons.None, string? message = null, string? detail = null, Exception? error = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        Success = success;
        Content = content;
        FailureReason = failureReason;
        ErrorMessage = message;
        ErrorDetail = detail;
        Error = error;
        ValidationErrors = validationErrors;
    }

    /// <inheritdoc />
    public bool TryGetContent([NotNullWhen(returnValue: true)] out T? content)
    {
        content = Content;
        return Success;
    }

    /// <summary>
    /// Creates a successful typed result with optional content.
    /// </summary>
    /// <param name="content">The content produced by the operation.</param>
    /// <returns>A successful <see cref="Result{T}"/> carrying the supplied content.</returns>
    public static Result<T> Ok(T? content = default)
        => new(success: true, content);

    /// <summary>
    /// Creates a failed typed result with a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied validation error.</returns>
    public static Result<T> Fail(int failureReason, ValidationError validationError)
        => new(false, failureReason: failureReason, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result with a message and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied message and validation error.</returns>
    public static Result<T> Fail(int failureReason, string? message, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result with a message, diagnostic detail, and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="detail">Additional diagnostic detail for logs or structured problem responses.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied message, detail, and validation error.</returns>
    public static Result<T> Fail(int failureReason, string? message, string? detail, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result that still carries content, such as partial data that an adapter may choose to expose.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="content">The content associated with the failed operation.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied content and validation error.</returns>
    public static Result<T> Fail(int failureReason, T content, ValidationError validationError)
        => new(false, failureReason: failureReason, content: content, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result with an exception and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="error">The exception that caused or explains the failure.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied exception and validation error.</returns>
    public static Result<T> Fail(int failureReason, Exception? error, ValidationError validationError)
        => new(false, failureReason: failureReason, error: error, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result with optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied failure reason and validation errors.</returns>
    public static Result<T> Fail(int failureReason, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed typed result with a message and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied message and validation errors.</returns>
    public static Result<T> Fail(int failureReason, string? message, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed typed result with a message, diagnostic detail, and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="detail">Additional diagnostic detail for logs or structured problem responses.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied message, detail, and validation errors.</returns>
    public static Result<T> Fail(int failureReason, string? message, string? detail, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed typed result that still carries content, such as partial data that an adapter may choose to expose.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="content">The content associated with the failed operation.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied content and validation errors.</returns>
    public static Result<T> Fail(int failureReason, T content, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, content: content, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed typed result with an exception and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="error">The exception that caused or explains the failure.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied exception and validation errors.</returns>
    public static Result<T> Fail(int failureReason, Exception? error, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, error: error, validationErrors: validationErrors);

    /// <summary>
    /// Converts a value into a successful <see cref="Result{T}"/> so service methods can return domain values directly while preserving the operation-result contract.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    public static implicit operator Result<T>(T value)
        => Ok(value);

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> into a typed result while preserving failure metadata.
    /// </summary>
    /// <param name="result">The non-generic result to convert.</param>
    public static implicit operator Result<T>(Result result)
        => new(result.Success, default, result.FailureReason, result.ErrorMessage, result.ErrorDetail, result.Error, result.ValidationErrors);

    /// <summary>
    /// Enables <see cref="Result{T}"/> instances to be used directly in conditional statements that branch on operation success.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns><see langword="true"/> when the operation succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool operator true(Result<T> result)
        => result.Success;

    /// <summary>
    /// Enables <see cref="Result{T}"/> instances to be used directly in conditional statements that branch on operation failure.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns><see langword="true"/> when the operation failed; otherwise, <see langword="false"/>.</returns>
    public static bool operator false(Result<T> result)
        => !result.Success;

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to a Boolean value that represents operation success.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns><see langword="true"/> when the operation succeeded; otherwise, <see langword="false"/>.</returns>
    public static implicit operator bool(Result<T> result)
        => result.Success;
}
