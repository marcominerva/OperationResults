namespace OperationResults;

/// <summary>
/// Represents the outcome of an operation that does not produce content.
/// </summary>
/// <remarks>
/// Use this type in application and business services to describe success or failure without coupling those services to HTTP responses, UI messages, or infrastructure-specific return types.
/// Presentation adapters can translate <see cref="FailureReason"/>, <see cref="ErrorMessage"/>, and <see cref="ValidationErrors"/> into their own response formats.
/// </remarks>
public class Result : IGenericResult
{
    /// <inheritdoc />
    public bool Success { get; }

    /// <inheritdoc />
    public int FailureReason { get; }

    /// <inheritdoc />
    public Exception? Error { get; }

    /// <inheritdoc />
    public string? ErrorMessage => field ?? Error?.Message;

    /// <inheritdoc />
    public string? ErrorDetail => field ?? Error?.InnerException?.Message;

    /// <inheritdoc />
    public IEnumerable<ValidationError>? ValidationErrors { get; }

    internal Result(bool success = true, int failureReason = FailureReasons.None, string? message = null, string? detail = null, Exception? error = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        Success = success;
        FailureReason = failureReason;
        ErrorMessage = message;
        ErrorDetail = detail;
        Error = error;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Creates a successful result for an operation that completed without returning content.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Ok()
        => new(success: true);

    /// <summary>
    /// Creates a failed result with a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied validation error.</returns>
    public static Result Fail(int failureReason, ValidationError validationError)
        => new(false, failureReason: failureReason, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed result with a message and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied message and validation error.</returns>
    public static Result Fail(int failureReason, string? message, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed result with a message, diagnostic detail, and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="detail">Additional diagnostic detail for logs or structured problem responses.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied message, detail, and validation error.</returns>
    public static Result Fail(int failureReason, string? message, string? detail, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed result with an exception and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="error">The exception that caused or explains the failure.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied exception and validation error.</returns>
    public static Result Fail(int failureReason, Exception? error, ValidationError validationError)
        => new(false, failureReason: failureReason, error: error, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed result with optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied failure reason and validation errors.</returns>
    public static Result Fail(int failureReason, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed result with a message and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied message and validation errors.</returns>
    public static Result Fail(int failureReason, string? message, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed result with a message, diagnostic detail, and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="detail">Additional diagnostic detail for logs or structured problem responses.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied message, detail, and validation errors.</returns>
    public static Result Fail(int failureReason, string? message, string? detail, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed result with an exception and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="error">The exception that caused or explains the failure.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied exception and validation errors.</returns>
    public static Result Fail(int failureReason, Exception? error, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, error: error, validationErrors: validationErrors);

    /// <summary>
    /// Enables <see cref="Result"/> instances to be used directly in conditional statements that branch on operation success.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns><see langword="true"/> when the operation succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool operator true(Result result)
        => result.Success;

    /// <summary>
    /// Enables <see cref="Result"/> instances to be used directly in conditional statements that branch on operation failure.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns><see langword="true"/> when the operation failed; otherwise, <see langword="false"/>.</returns>
    public static bool operator false(Result result)
        => !result.Success;

    /// <summary>
    /// Converts a <see cref="Result"/> to a Boolean value that represents operation success.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns><see langword="true"/> when the operation succeeded; otherwise, <see langword="false"/>.</returns>
    public static implicit operator bool(Result result)
        => result.Success;
}
