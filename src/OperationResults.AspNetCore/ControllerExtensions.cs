using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace OperationResults.AspNetCore;

public static class ControllerExtensions
{
    public static IActionResult CreateResponse(this HttpContext httpContext, Result operationResult, int? responseStatusCode = null)
    {
        if (operationResult.Success)
        {
            return new StatusCodeResult(responseStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent));
        }

        return Problem(httpContext, FailureReasonToStatusCode(httpContext, operationResult.FailureReason), null, operationResult.ErrorMessage, operationResult.ErrorDetail, operationResult.ValidationErrors);
    }

    public static IActionResult CreateResponse<T>(this HttpContext httpContext, Result<T> operationResult, int? responseStatusCode = null)
        => CreateResponse(httpContext, operationResult, null, null, responseStatusCode);

    public static IActionResult CreateResponse<T>(this HttpContext httpContext, Result<T> operationResult, string? actionName, object? routeValues = null, int? responseStatusCode = null)
    {
        if (operationResult.Success)
        {
            if (operationResult.Content is not null)
            {
                if (!string.IsNullOrWhiteSpace(actionName))
                {
                    var routeValueDictionary = new RouteValueDictionary(routeValues);
                    //var apiVersion = HttpContext.GetRequestedApiVersion();
                    //if (!routeValueDictionary.ContainsKey("version") && apiVersion != null)
                    //{
                    //    routeValueDictionary.Add("version", apiVersion.ToString());
                    //}

                    return new CreatedAtRouteResult(actionName, routeValueDictionary, operationResult.Content);
                }
                else if (operationResult.Content is StreamFileContent streamFileContent)
                {
                    var fileStreamResult = new FileStreamResult(streamFileContent.Content, streamFileContent.ContentType)
                    {
                        FileDownloadName = streamFileContent.DownloadFileName
                    };

                    return fileStreamResult;
                }
                else if (operationResult.Content is ByteArrayFileContent byteArrayFileContent)
                {
                    var fileContentResult = new FileContentResult(byteArrayFileContent.Content, byteArrayFileContent.ContentType)
                    {
                        FileDownloadName = byteArrayFileContent.DownloadFileName
                    };

                    return fileContentResult;
                }

                var okResult = new ObjectResult(operationResult.Content)
                {
                    StatusCode = responseStatusCode.GetValueOrDefault(StatusCodes.Status200OK)
                };

                return okResult;
            }

            return new StatusCodeResult(responseStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent));
        }

        return Problem(httpContext, FailureReasonToStatusCode(httpContext, operationResult.FailureReason), operationResult.Content, operationResult.ErrorMessage, operationResult.ErrorDetail, operationResult.ValidationErrors);
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
            problemDetails.Extensions.Add("errors", validationErrors);
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
