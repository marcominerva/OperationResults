using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OperationResults;

/// <summary>
/// Represents the outcome of an operation that does not produce content.
/// </summary>
/// <remarks>
/// <para>
/// Use this type in application and business services to describe success or failure without coupling those services to HTTP responses, UI messages, or infrastructure-specific return types.
/// </para>
/// <para>
/// This is a <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union#custom-union-types">custom union type</see> whose case types are
/// <see cref="bool"/> and <see cref="Error"/>. When it doesn't carry an <see cref="Error"/>, the result is always a <see cref="bool"/>:
/// a <see langword="default"/> instance (for example, an uninitialized field or array element) is equivalent to <c>new Result(false)</c>, so it is never considered successful.
/// </para>
/// </remarks>
/// <example>
/// <code language="csharp">
/// Result result = await service.DeleteAsync(id);
///
/// if (!result)
/// {
///     logger.LogWarning("Delete failed");
/// }
/// </code>
/// </example>
[Union]
public readonly struct Result : IUnion
{
    private static readonly object boxedTrue = true;
    private static readonly object boxedFalse = false;

    private readonly Error? error;

    /// <summary>
    /// Initializes a <see cref="Result"/> from a Boolean outcome.
    /// </summary>
    /// <param name="value">
    /// <see langword="true"/> for a successful operation; <see langword="false"/> for an unsuccessful operation that carries no <see cref="Error"/> details.
    /// Prefer <see cref="Ok"/> and the <c>Fail</c> overloads to create results.
    /// </param>
    public Result(bool value)
    {
        Success = value;
    }

    /// <summary>
    /// Initializes a failed <see cref="Result"/> that carries the specified error.
    /// </summary>
    /// <param name="error">The error that describes why the operation failed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        this.error = error;
    }

    /// <summary>
    /// Gets the case value stored in the union: the <see cref="Error"/> when the operation failed with an error; otherwise, the <see cref="bool"/> outcome.
    /// </summary>
    /// <remarks>The <see cref="bool"/> case uses cached boxes, so reading this property never allocates.</remarks>
    public object? Value => error ?? (Success ? boxedTrue : boxedFalse);

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is not <see langword="null"/>, which is always <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The compiler uses this member to evaluate the <see langword="null"/> pattern, which never matches a <see cref="Result"/>. It doesn't indicate success: use <see cref="Success"/> for that purpose.
    /// </remarks>
    public bool HasValue => true;

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully from the business perspective.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="false"/> for a failed result, for a result created from <see langword="false"/>, and for a <see langword="default"/> instance.
    /// </remarks>
    public bool Success { get; }

    /// <summary>
    /// Attempts to get the Boolean case value.
    /// </summary>
    /// <param name="value">When this method returns <see langword="true"/>, the Boolean outcome of the operation.</param>
    /// <returns><see langword="true"/> when the result doesn't carry an <see cref="Error"/>, including a <see langword="default"/> instance; otherwise, <see langword="false"/>.</returns>
    /// <remarks>The compiler uses this member to evaluate type patterns on <see cref="bool"/>.</remarks>
    public bool TryGetValue(out bool value)
    {
        value = Success;
        return error is null;
    }

    /// <summary>
    /// Attempts to get the error of a failed operation.
    /// </summary>
    /// <param name="value">When this method returns <see langword="true"/>, the error that describes the failure.</param>
    /// <returns><see langword="true"/> when the result carries an <see cref="Error"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>The compiler uses this member to evaluate type patterns on <see cref="Error"/>.</remarks>
    public bool TryGetValue([MaybeNullWhen(false)] out Error value)
    {
        value = error;
        return value is not null;
    }

    /// <summary>
    /// Creates a successful result for an operation that completed without returning content.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Ok()
        => true;

    /// <summary>
    /// Creates a failed result with a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied validation error.</returns>
    public static Result Fail(int failureReason, ValidationError validationError)
        => new Error(failureReason: failureReason, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed result with a message and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied message and validation error.</returns>
    public static Result Fail(int failureReason, string? message, ValidationError validationError)
        => new Error(failureReason: failureReason, message: message, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed result with a message, diagnostic detail, and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="detail">Additional diagnostic detail for logs or structured problem responses.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied message, detail, and validation error.</returns>
    public static Result Fail(int failureReason, string? message, string? detail, ValidationError validationError)
        => new Error(failureReason: failureReason, message: message, detail: detail, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed result with an exception and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="exception">The exception that caused or explains the failure.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied exception and validation error.</returns>
    public static Result Fail(int failureReason, Exception? exception, ValidationError validationError)
        => new Error(failureReason: failureReason, exception: exception, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed result with optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied failure reason and validation errors.</returns>
    public static Result Fail(int failureReason, IEnumerable<ValidationError>? validationErrors = null)
        => new Error(failureReason: failureReason, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed result with a message and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied message and validation errors.</returns>
    public static Result Fail(int failureReason, string? message, IEnumerable<ValidationError>? validationErrors = null)
        => new Error(failureReason: failureReason, message: message, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed result with a message, diagnostic detail, and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="detail">Additional diagnostic detail for logs or structured problem responses.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied message, detail, and validation errors.</returns>
    public static Result Fail(int failureReason, string? message, string? detail, IEnumerable<ValidationError>? validationErrors = null)
        => new Error(failureReason: failureReason, message: message, detail: detail, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed result with an exception and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="exception">The exception that caused or explains the failure.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result"/> carrying the supplied exception and validation errors.</returns>
    public static Result Fail(int failureReason, Exception? exception, IEnumerable<ValidationError>? validationErrors = null)
        => new Error(failureReason: failureReason, exception: exception, validationErrors: validationErrors);

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
    /// <returns><see langword="true"/> when the operation did not succeed, including a <see langword="default"/> instance, which is equivalent to <see langword="false"/>; otherwise, <see langword="false"/>.</returns>
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
