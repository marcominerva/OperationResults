using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace OperationResults.AspNetCore.Http;

public static class OperationResultExtensions
{
    public static IResult ToResponse(this Result result, HttpContext httpContext, int? successStatusCode = null)
    {
        if (result.Success)
        {
            return TypedResults.StatusCode(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent));
        }

        return Problem(httpContext, result.FailureReason, null, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
    }

    public static IResult ToResponse(this Result result, HttpContext httpContext, string? routeName, object? routeValues = null)
    {
        if (result.Success)
        {
            var routeValueDictionary = new RouteValueDictionary(routeValues);
            return TypedResults.CreatedAtRoute(routeName, routeValues);
        }

        return Problem(httpContext, result.FailureReason, null, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
    }

    public static IResult ToResponse<T>(this Result<T> result, HttpContext httpContext, int? successStatusCode = null)
        => result.ToResponse(httpContext, null, null, successStatusCode);

    public static IResult ToResponse<T>(this Result<T> result, HttpContext httpContext, string? routeName, object? routeValues = null, int? successStatusCode = null)
    {
        if (result.Success)
        {
            if (result.Content is not null)
            {
                if (!string.IsNullOrWhiteSpace(routeName))
                {
                    var routeValueDictionary = new RouteValueDictionary(routeValues);
                    return TypedResults.CreatedAtRoute(result.Content, routeName, routeValues);
                }
                else if (result.Content is StreamFileContent streamFileContent)
                {
                    return TypedResults.Stream(streamFileContent.Content, streamFileContent.ContentType, streamFileContent.DownloadFileName);
                }
                else if (result.Content is ByteArrayFileContent byteArrayFileContent)
                {
                    return TypedResults.File(byteArrayFileContent.Content, byteArrayFileContent.ContentType, byteArrayFileContent.DownloadFileName);
                }

                return TypedResults.Json(result.Content, statusCode: successStatusCode.GetValueOrDefault(StatusCodes.Status200OK));
            }

            return TypedResults.StatusCode(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent));
        }

        return Problem(httpContext, result.FailureReason, result.Content, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
    }

    private static IResult Problem(HttpContext httpContext, int failureReason, object? content = null, string? title = null, string? detail = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        var options = httpContext.RequestServices.GetService<OperationResultOptions>() ?? new OperationResultOptions();
        var statusCode = options.MapStatusCodes ? options.GetStatusCode(failureReason) : failureReason;

        if (content is not null)
        {
            return TypedResults.Json(content, statusCode: statusCode);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title ?? ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);

        if (validationErrors?.Any() ?? false)
        {
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

        var problemResult = TypedResults.Problem(problemDetails);
        problemResult.ProblemDetails.Type ??= $"https://httpstatuses.io/{statusCode}";

        return problemResult;
    }
}
