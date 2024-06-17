using Microsoft.AspNetCore.Http;

namespace OperationResults.AspNetCore;

public class OperationResultOptions
{
    public ErrorResponseFormat ErrorResponseFormat { get; set; } = ErrorResponseFormat.Default;

    public Dictionary<int, int> StatusCodesMapping { get; }

    public bool MapStatusCodes { get; set; } = true;

    public UnmappedFailureReasonBehavior UnmappedFailureReasonBehavior { get; set; } = UnmappedFailureReasonBehavior.UseDefaultStatusCode;

    public int UnmappedFailureReasonStatusCode { get; set; } = StatusCodes.Status501NotImplemented;

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
