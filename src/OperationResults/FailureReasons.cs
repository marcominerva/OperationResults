namespace OperationResults;

/// <summary>
/// Defines the built-in failure reason codes used by <see cref="Result"/> and <see cref="Result{T}"/> to describe why an operation did not complete successfully.
/// </summary>
/// <remarks>
/// These values intentionally avoid transport-specific concepts so application and domain services can return failures without depending on HTTP, UI, or persistence concerns.
/// Integration layers can map the codes to protocol-specific responses, such as HTTP status codes in ASP.NET Core minimal APIs.
/// </remarks>
public static class FailureReasons
{
    /// <summary>
    /// Indicates that no failure reason is associated with the operation result.
    /// </summary>
    public const int None = 0;

    /// <summary>
    /// Indicates that the caller supplied data that cannot be accepted by the operation.
    /// </summary>
    public const int ClientError = 1;

    /// <summary>
    /// Indicates that the operation requires an authenticated caller.
    /// </summary>
    public const int Unauthorized = 2;

    /// <summary>
    /// Indicates that the caller is authenticated but is not allowed to perform the operation.
    /// </summary>
    public const int Forbidden = 3;

    /// <summary>
    /// Indicates that the requested resource or entity could not be found.
    /// </summary>
    public const int ItemNotFound = 4;

    /// <summary>
    /// Indicates that the request shape or arguments are invalid for the operation being executed.
    /// </summary>
    public const int InvalidRequest = 5;

    /// <summary>
    /// Indicates that the operation did not complete within the expected time budget.
    /// </summary>
    public const int Timeout = 6;

    /// <summary>
    /// Indicates that the operation conflicts with the current state of the target resource.
    /// </summary>
    public const int Conflict = 7;

    /// <summary>
    /// Indicates that a supplied file is missing, malformed, unsupported, or unsafe to process.
    /// </summary>
    public const int InvalidFile = 8;

    /// <summary>
    /// Indicates that supplied content cannot be parsed, validated, or processed by the operation.
    /// </summary>
    public const int InvalidContent = 9;

    /// <summary>
    /// Indicates that a data store operation failed.
    /// </summary>
    public const int DatabaseError = 10;

    /// <summary>
    /// Indicates that a network dependency prevented the operation from completing successfully.
    /// </summary>
    public const int NetworkError = 11;

    /// <summary>
    /// Indicates that an external or internal service required by the operation is temporarily unavailable.
    /// </summary>
    public const int ServiceUnavailable = 12;

    /// <summary>
    /// Indicates an application-defined failure that does not fit a more specific built-in reason.
    /// </summary>
    public const int GenericError = 1000;
}
