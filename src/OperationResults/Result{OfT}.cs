using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OperationResults;

/// <summary>
/// Represents the outcome of an operation that can produce content when it succeeds.
/// </summary>
/// <typeparam name="T">The type of content returned by the operation.</typeparam>
/// <remarks>
/// <para>
/// Use this type in application and business services to return values together with failure metadata while remaining independent from protocol-specific response types.
/// This allows adapters, such as ASP.NET Core endpoints, to translate the same business result into the response format required by the host application.
/// </para>
/// <para>
/// This is a <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union#custom-union-types">custom union type</see> whose case types are
/// <typeparamref name="T"/> and <see cref="Error"/>. Unlike a compiler-generated union, it stores an explicit discriminator instead of inferring the case from the runtime type of
/// <see cref="Value"/>. This makes a successful operation whose content is <see langword="null"/> distinguishable from a <see langword="default"/> instance
/// (for example, an uninitialized field or array element), which is never considered successful.
/// </para>
/// <para>
/// Because type patterns never match <see langword="null"/>, a successful result with <see langword="null"/> content matches the <see langword="null"/> pattern.
/// Use <see cref="Success"/> when that case must be told apart from a <see langword="default"/> instance.
/// </para>
/// </remarks>
/// <example>
/// <code language="csharp">
/// Result&lt;Person&gt; result = await service.GetAsync(id);
///
/// if (result)
/// {
///     Console.WriteLine(result.Content?.Name);
/// }
///
/// var message = result switch
/// {
///     Person person =&gt; person.Name,
///     Error error =&gt; error.ErrorMessage,
///     null =&gt; null
/// };
/// </code>
/// </example>
[Union]
public readonly struct Result<T> : IUnion
{
    private readonly T? content;
    private readonly Error? error;

    /// <summary>
    /// Initializes a successful <see cref="Result{T}"/> that carries the specified content.
    /// </summary>
    /// <param name="content">
    /// The content produced by the operation. It can be <see langword="null"/> when <typeparamref name="T"/> allows it: the result is still successful.
    /// </param>
    /// <remarks>
    /// When <typeparamref name="T"/> is <see cref="object"/>, an <see cref="Error"/> passed as content creates a failed result, exactly like <see cref="Result{T}(Error)"/>.
    /// This guarantees that an <see cref="Error"/> always represents a failure, so <see cref="Value"/>, <see cref="Success"/>, and pattern matching never disagree.
    /// </remarks>
    public Result(T content)
    {
        if (content is Error error)
        {
            this.error = error;
        }
        else
        {
            this.content = content;
            Success = true;
        }
    }

    /// <summary>
    /// Initializes a failed <see cref="Result{T}"/> that carries the specified error.
    /// </summary>
    /// <param name="error">The error that describes why the operation failed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        this.error = error;
    }

    /// <summary>
    /// Gets the case value stored in the union: the content when the operation succeeded, the <see cref="Error"/> when it failed,
    /// or <see langword="null"/> for successful <see langword="null"/> content and for a <see langword="default"/> instance.
    /// </summary>
    public object? Value => Success ? content : error;

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// The compiler uses this member to evaluate the <see langword="null"/> pattern without boxing. It doesn't indicate success: use <see cref="Success"/> for that purpose.
    /// </remarks>
    public bool HasValue => Success ? content is not null : error is not null;

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully from the business perspective.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> also when the operation succeeded with <see langword="null"/> content, and <see langword="false"/> for a <see langword="default"/> instance.
    /// </remarks>
    public bool Success { get; }

    /// <summary>
    /// Gets the content produced by a successful operation.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="default"/> for a <see langword="default"/> instance, matching the behavior of an uninitialized value. Check <see cref="Success"/> to tell it apart from a successful operation.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The operation failed.</exception>
    public T? Content => error is null
        ? content
        : throw new InvalidOperationException($"Cannot access {nameof(Content)} when the operation failed. Check {nameof(Success)} before accessing the content.");

    /// <summary>
    /// Attempts to get the content of a successful operation without boxing it.
    /// </summary>
    /// <param name="value">When this method returns <see langword="true"/>, the content of the operation.</param>
    /// <returns>
    /// <see langword="true"/> when the operation succeeded with non-<see langword="null"/> content; otherwise, <see langword="false"/>.
    /// When <typeparamref name="T"/> is <see cref="object"/>, it also returns <see langword="true"/> for a failed operation, delivering the <see cref="Error"/>, to stay consistent with <see cref="Value"/>.
    /// </returns>
    /// <remarks>The compiler uses this member to evaluate type patterns on <typeparamref name="T"/>.</remarks>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        if (Success && content is not null)
        {
            value = content;
            return true;
        }

        if (error is T errorValue)
        {
            value = errorValue;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the error of a failed operation.
    /// </summary>
    /// <param name="value">When this method returns <see langword="true"/>, the error that describes the failure.</param>
    /// <returns><see langword="true"/> when the operation failed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>The compiler uses this member to evaluate type patterns on <see cref="Error"/>.</remarks>
    public bool TryGetValue([MaybeNullWhen(false)] out Error value)
    {
        value = error;
        return value is not null;
    }

    /// <summary>
    /// Creates a failed typed result with a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied validation error.</returns>
    public static Result<T> Fail(int failureReason, ValidationError validationError)
        => new Error(failureReason: failureReason, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result with a message and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied message and validation error.</returns>
    public static Result<T> Fail(int failureReason, string? message, ValidationError validationError)
        => new Error(failureReason: failureReason, message: message, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result with a message, diagnostic detail, and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="detail">Additional diagnostic detail for logs or structured problem responses.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied message, detail, and validation error.</returns>
    public static Result<T> Fail(int failureReason, string? message, string? detail, ValidationError validationError)
        => new Error(failureReason: failureReason, message: message, detail: detail, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result with an exception and a single validation error.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="exception">The exception that caused or explains the failure.</param>
    /// <param name="validationError">The validation error associated with the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied exception and validation error.</returns>
    public static Result<T> Fail(int failureReason, Exception? exception, ValidationError validationError)
        => new Error(failureReason: failureReason, exception: exception, validationErrors: [validationError]);

    /// <summary>
    /// Creates a failed typed result with optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied failure reason and validation errors.</returns>
    public static Result<T> Fail(int failureReason, IEnumerable<ValidationError>? validationErrors = null)
        => new Error(failureReason: failureReason, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed typed result with a message and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied message and validation errors.</returns>
    public static Result<T> Fail(int failureReason, string? message, IEnumerable<ValidationError>? validationErrors = null)
        => new Error(failureReason: failureReason, message: message, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed typed result with a message, diagnostic detail, and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="message">A message that adapters can expose in an error response or log entry.</param>
    /// <param name="detail">Additional diagnostic detail for logs or structured problem responses.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied message, detail, and validation errors.</returns>
    public static Result<T> Fail(int failureReason, string? message, string? detail, IEnumerable<ValidationError>? validationErrors = null)
        => new Error(failureReason: failureReason, message: message, detail: detail, validationErrors: validationErrors);

    /// <summary>
    /// Creates a failed typed result with an exception and optional validation errors.
    /// </summary>
    /// <param name="failureReason">The application-level reason that explains the failure.</param>
    /// <param name="exception">The exception that caused or explains the failure.</param>
    /// <param name="validationErrors">The validation errors associated with the failure, if any.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the supplied exception and validation errors.</returns>
    public static Result<T> Fail(int failureReason, Exception? exception, IEnumerable<ValidationError>? validationErrors = null)
        => new Error(failureReason: failureReason, exception: exception, validationErrors: validationErrors);

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> into a typed result, so methods that return <see cref="Result{T}"/> can use <see cref="Result.Fail(int, IEnumerable{ValidationError}?)"/> and its overloads.
    /// </summary>
    /// <param name="result">The non-generic result to convert.</param>
    /// <returns>
    /// A failed <see cref="Result{T}"/> carrying the same <see cref="Error"/> instance when <paramref name="result"/> failed with an error;
    /// a successful <see cref="Result{T}"/> with <see langword="default"/> content when <paramref name="result"/> succeeded;
    /// otherwise (a result created from <see langword="false"/>, including a <see langword="default"/> instance), a <see langword="default"/> <see cref="Result{T}"/>, which is not successful.
    /// </returns>
    /// <example>
    /// <code language="csharp">
    /// public Result&lt;Person&gt; Get(Guid id)
    ///     =&gt; people.TryGetValue(id, out var person) ? person : Result.Fail(FailureReasons.ItemNotFound);
    /// </code>
    /// </example>
    public static implicit operator Result<T>(Result result)
        => result switch
        {
            Error error => new(error),
            true => new(default(T)!),
            _ => default
        };

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
    /// <returns><see langword="true"/> when the operation did not succeed, including a <see langword="default"/> instance; otherwise, <see langword="false"/>.</returns>
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
