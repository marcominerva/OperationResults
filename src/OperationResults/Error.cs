using System.Diagnostics.CodeAnalysis;

namespace OperationResults;

public class Error 
{
    /// <summary>
    /// Gets the application-level failure code that explains unsuccessful operations and can be mapped by adapters to protocol-specific responses.
    /// </summary>
    public int FailureReason { get; }

    /// <summary>
    /// Gets the exception captured for diagnostics when a failure is caused by an exceptional condition.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets a value indicating whether the result carries an exception for diagnostic purposes.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Exception))]
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool HasException => Exception is not null;

    /// <summary>
    /// Gets an optional human-readable failure message suitable for adapter-specific error payloads.
    /// </summary>
    public string? ErrorMessage => field ?? Exception?.Message;

    /// <summary>
    /// Gets optional low-level diagnostic detail that can help logs or problem responses explain the failure cause.
    /// </summary>
    public string? ErrorDetail => field ?? Exception?.InnerException?.Message;

    /// <summary>
    /// Gets field or rule validation errors that explain why caller-supplied data was rejected.
    /// </summary>
    public IEnumerable<ValidationError>? ValidationErrors { get; }

    internal Error(int failureReason = FailureReasons.None, string? message = null, string? detail = null, Exception? exception = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        FailureReason = failureReason;
        ErrorMessage = message;
        ErrorDetail = detail;
        Exception = exception;
        ValidationErrors = validationErrors;
    }
}
