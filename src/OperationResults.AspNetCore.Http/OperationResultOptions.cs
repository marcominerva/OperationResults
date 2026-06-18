using Microsoft.AspNetCore.Http;

namespace OperationResults.AspNetCore.Http;

/// <summary>
/// Configures how <see cref="Result"/> and <see cref="Result{T}"/> values are translated into ASP.NET Core Minimal API <see cref="IResult"/> responses.
/// </summary>
/// <remarks>
/// Register and customize this type through <see cref="ServiceCollectionExtensions.AddOperationResult"/> so Minimal API endpoints can map business-layer result codes to HTTP responses consistently.
/// </remarks>
public class OperationResultOptions
{
    /// <summary>
    /// Gets or sets the shape used for validation errors written to problem details responses.
    /// </summary>
    public ErrorResponseFormat ErrorResponseFormat { get; set; } = ErrorResponseFormat.Default;

    /// <summary>
    /// Gets the mapping between application-level failure reasons and HTTP status codes.
    /// </summary>
    /// <remarks>
    /// Applications can add or replace entries to keep domain and application services focused on <see cref="FailureReasons"/> or custom failure codes while Minimal API endpoints return protocol-appropriate status codes.
    /// </remarks>
    public Dictionary<int, int> StatusCodesMapping { get; }

    /// <summary>
    /// Gets or sets a value indicating whether failure reasons are translated through <see cref="StatusCodesMapping"/> before writing the HTTP response.
    /// </summary>
    /// <remarks>
    /// Set this value to <see langword="false"/> when the application intentionally uses HTTP status codes directly as <see cref="Result.FailureReason"/> values.
    /// </remarks>
    public bool MapStatusCodes { get; set; } = true;

    /// <summary>
    /// Gets or sets the behavior used when <see cref="MapStatusCodes"/> is enabled and a failure reason has no configured mapping.
    /// </summary>
    public UnmappedFailureReasonBehavior UnmappedFailureReasonBehavior { get; set; } = UnmappedFailureReasonBehavior.UseDefaultStatusCode;

    /// <summary>
    /// Gets or sets the HTTP status code returned for unmapped failure reasons when <see cref="UnmappedFailureReasonBehavior"/> is <see cref="Http.UnmappedFailureReasonBehavior.UseDefaultStatusCode"/>.
    /// </summary>
    public int UnmappedFailureReasonStatusCode { get; set; } = StatusCodes.Status501NotImplemented;

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResultOptions"/> class with the default failure-reason to HTTP-status-code mappings.
    /// </summary>
    public OperationResultOptions()
    {
        StatusCodesMapping = new Dictionary<int, int>
        {
            [FailureReasons.None] = StatusCodes.Status200OK,
            [FailureReasons.ClientError] = StatusCodes.Status400BadRequest,
            [FailureReasons.Unauthorized] = StatusCodes.Status401Unauthorized,
            [FailureReasons.Forbidden] = StatusCodes.Status403Forbidden,
            [FailureReasons.ItemNotFound] = StatusCodes.Status404NotFound,
            [FailureReasons.InvalidRequest] = StatusCodes.Status406NotAcceptable,
            [FailureReasons.Timeout] = StatusCodes.Status408RequestTimeout,
            [FailureReasons.Conflict] = StatusCodes.Status409Conflict,
            [FailureReasons.InvalidFile] = StatusCodes.Status415UnsupportedMediaType,
            [FailureReasons.InvalidContent] = StatusCodes.Status422UnprocessableEntity,
            [FailureReasons.DatabaseError] = StatusCodes.Status500InternalServerError,
            [FailureReasons.NetworkError] = StatusCodes.Status500InternalServerError,
            [FailureReasons.ServiceUnavailable] = StatusCodes.Status503ServiceUnavailable,
            [FailureReasons.GenericError] = StatusCodes.Status500InternalServerError
        };
    }

    internal int GetStatusCode(int failureReason, int? defaultStatusCode = null)
    {
        if (!StatusCodesMapping.TryGetValue(failureReason, out var statusCode))
        {
            statusCode = UnmappedFailureReasonBehavior switch
            {
                UnmappedFailureReasonBehavior.UseDefaultStatusCode => defaultStatusCode.GetValueOrDefault(UnmappedFailureReasonStatusCode),
                _ => failureReason
            };
        }

        return statusCode;
    }
}
