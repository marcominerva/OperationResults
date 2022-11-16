using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace OperationResults.AspNetCore;

public static class OperationResultExtensions
{
    public static IActionResult ToResponse(this Result result, HttpContext httpContext, int? successStatusCode = null)
    {
        if (result.Success)
        {
            return new StatusCodeResult(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent));
        }

        return Problem(httpContext, FailureReasonToStatusCode(httpContext, result.FailureReason), null, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
    }

    public static IActionResult ToResponse<T>(this Result<T> result, HttpContext httpContext, int? successStatusCode = null)
        => result.ToResponse(httpContext, null, null, successStatusCode);

    public static IActionResult ToResponse<T>(this Result<T> result, HttpContext httpContext, string? routeName, object? routeValues = null, int? successStatusCode = null)
    {
        if (result.Success)
        {
            if (result.Content is not null)
            {
                if (!string.IsNullOrWhiteSpace(routeName))
                {
                    var createdAtRouteResult = new CreatedAtRouteResult(routeName, routeValues, result.Content)
                    {
                        StatusCode = successStatusCode.GetValueOrDefault(StatusCodes.Status201Created)
                    };

                    return createdAtRouteResult;
                }
                else if (result.Content is StreamFileContent streamFileContent)
                {
                    var fileStreamResult = new FileStreamResult(streamFileContent.Content, streamFileContent.ContentType)
                    {
                        FileDownloadName = streamFileContent.DownloadFileName,
                    };

                    return fileStreamResult;
                }
                else if (result.Content is ByteArrayFileContent byteArrayFileContent)
                {
                    var fileContentResult = new FileContentResult(byteArrayFileContent.Content, byteArrayFileContent.ContentType)
                    {
                        FileDownloadName = byteArrayFileContent.DownloadFileName
                    };

                    return fileContentResult;
                }

                var okResult = new ObjectResult(result.Content)
                {
                    StatusCode = successStatusCode.GetValueOrDefault(StatusCodes.Status200OK)
                };

                return okResult;
            }

            return new StatusCodeResult(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent));
        }

        return Problem(httpContext, FailureReasonToStatusCode(httpContext, result.FailureReason), result.Content, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
    }

    private static IActionResult Problem(HttpContext httpContext, int statusCode, object? content = null, string? title = null, string? detail = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        if (content is not null)
        {
            var objectResult = new ObjectResult(content)
            {
                StatusCode = statusCode
            };

            return objectResult;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = title ?? ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);

        if (validationErrors?.Any() ?? false)
        {
            var options = httpContext.RequestServices.GetRequiredService<OperationResultOptions>();

            if (options.ErrorResponseFormat == ErrorResponseFormat.Default)
            {
                var errors = validationErrors.GroupBy(v => v.Name).ToDictionary(k => k.Key, v => v.Select(e => e.Message));
                problemDetails.Extensions.Add("errors", errors);
            }
            else
            {
                problemDetails.Extensions.Add("errors", validationErrors);
            }
        }

        var problemDetailsResults = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };

        return problemDetailsResults;
    }

    private static int FailureReasonToStatusCode(HttpContext httpContext, int failureReason, int? defaultResponseStatusCode = null)
    {
        var options = httpContext.RequestServices.GetRequiredService<OperationResultOptions>();
        var statusCode = options.GetStatusCode(failureReason, defaultResponseStatusCode);

        return statusCode;
    }
}
