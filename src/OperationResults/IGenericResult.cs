using System.Diagnostics.CodeAnalysis;

namespace OperationResults;

/// <summary>
/// Exposes the transport-neutral metadata shared by all operation results.
/// </summary>
/// <remarks>
/// Use this interface when code needs to inspect success, failure reason, and diagnostic information without knowing whether the result carries content.
/// </remarks>
public interface IGenericResult
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully from the business perspective.
    /// </summary>
    bool Success { get; }

    /// <summary>
    /// Gets the application-level failure code that explains unsuccessful operations and can be mapped by adapters to protocol-specific responses.
    /// </summary>
    int FailureReason { get; }

    /// <summary>
    /// Gets the exception captured for diagnostics when a failure is caused by an exceptional condition.
    /// </summary>
    Exception? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the result carries an exception for diagnostic purposes.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    bool HasError => Error is not null;

    /// <summary>
    /// Gets optional low-level diagnostic detail that can help logs or problem responses explain the failure cause.
    /// </summary>
    string? ErrorDetail { get; }

    /// <summary>
    /// Gets an optional human-readable failure message suitable for adapter-specific error payloads.
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// Gets field or rule validation errors that explain why caller-supplied data was rejected.
    /// </summary>
    IEnumerable<ValidationError>? ValidationErrors { get; }
}
