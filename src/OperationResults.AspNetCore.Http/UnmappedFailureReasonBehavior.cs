namespace OperationResults.AspNetCore.Http;

/// <summary>
/// Defines how Minimal API response mapping handles a <see cref="Result.FailureReason"/> value that is not present in <see cref="OperationResultOptions.StatusCodesMapping"/>.
/// </summary>
/// <remarks>
/// This setting is useful when applications introduce custom failure reasons and need to decide whether unmapped values are treated as application codes or direct HTTP status codes.
/// </remarks>
public enum UnmappedFailureReasonBehavior
{
    /// <summary>
    /// Uses <see cref="OperationResultOptions.UnmappedFailureReasonStatusCode"/> or the caller-provided default status code.
    /// </summary>
    UseDefaultStatusCode,

    /// <summary>
    /// Treats the failure reason value as the HTTP status code to return.
    /// </summary>
    UseFailureReason
}
