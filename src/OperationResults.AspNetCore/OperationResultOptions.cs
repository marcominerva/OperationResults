using Microsoft.AspNetCore.Http;

namespace OperationResults.AspNetCore;

public class OperationResultOptions
{
    public Dictionary<int, int> StatusCodesMapping { get; set; }

    public OperationResultOptions()
    {
        StatusCodesMapping = new Dictionary<int, int>
        {
            [FailureReasons.None] = StatusCodes.Status200OK,
            [FailureReasons.ItemNotFound] = StatusCodes.Status404NotFound,
            [FailureReasons.Forbidden] = StatusCodes.Status403Forbidden,
            [FailureReasons.DatabaseError] = StatusCodes.Status500InternalServerError,
            [FailureReasons.ClientError] = StatusCodes.Status400BadRequest,
            [FailureReasons.InvalidFile] = StatusCodes.Status415UnsupportedMediaType,
            [FailureReasons.Conflict] = StatusCodes.Status409Conflict,
            [FailureReasons.Unauthorized] = StatusCodes.Status401Unauthorized,
            [FailureReasons.GenericError] = StatusCodes.Status500InternalServerError
        };
    }

    internal int GetStatusCode(int failureReason, int? defaultStatusCode = null)
    {
        if (!StatusCodesMapping.TryGetValue(failureReason, out var statusCode))
        {
            statusCode = defaultStatusCode.GetValueOrDefault(StatusCodes.Status501NotImplemented);
        }

        return statusCode;
    }
}
