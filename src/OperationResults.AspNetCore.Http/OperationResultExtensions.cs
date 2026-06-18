using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace OperationResults.AspNetCore.Http;

/// <summary>
/// Provides extension methods that convert operation results into ASP.NET Core Minimal API <see cref="IResult"/> instances.
/// </summary>
/// <remarks>
/// These methods are the Minimal API adapter between transport-neutral business results and HTTP responses. They centralize status-code mapping, problem details creation, validation error formatting, and file-result handling so endpoint delegates can stay focused on orchestration.
/// </remarks>
public static class OperationResultExtensions
{
    /// <summary>
    /// Converts a non-generic operation result into a Minimal API response.
    /// </summary>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; the default is <see cref="StatusCodes.Status204NoContent"/>.</param>
    /// <returns>An <see cref="IResult"/> that represents the operation outcome.</returns>
    public static IResult ToResponse(this Result result, HttpContext httpContext, int? successStatusCode = null)
    {
        if (result.Success)
        {
            return TypedResults.StatusCode(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent));
        }

        return Problem(httpContext, result.FailureReason, null, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
    }

    /// <summary>
    /// Converts a non-generic operation result into a Minimal API route-based response when it succeeds, or a problem response when it fails.
    /// </summary>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="routeName">The route name used to build the location of the created resource.</param>
    /// <param name="routeValues">The route values used with <paramref name="routeName"/>.</param>
    /// <remarks>The success response uses <see cref="StatusCodes.Status201Created"/>.</remarks>
    /// <returns>An <see cref="IResult"/> that represents the operation outcome.</returns>
    public static IResult ToResponse(this Result result, HttpContext httpContext, string? routeName, object? routeValues = null)
    {
        if (result.Success)
        {
            var routeValueDictionary = new RouteValueDictionary(routeValues);
            return TypedResults.CreatedAtRoute(routeName, routeValues);
        }

        return Problem(httpContext, result.FailureReason, null, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
    }

    /// <summary>
    /// Converts a typed operation result into a Minimal API response.
    /// </summary>
    /// <typeparam name="T">The type of content carried by the operation result.</typeparam>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; the default is <see cref="StatusCodes.Status200OK"/> when content exists or <see cref="StatusCodes.Status204NoContent"/> when it does not.</param>
    /// <returns>An <see cref="IResult"/> that represents the operation outcome.</returns>
    public static IResult ToResponse<T>(this Result<T> result, HttpContext httpContext, int? successStatusCode = null)
        => result.ToResponse(httpContext, null, null, successStatusCode);

    /// <summary>
    /// Converts a typed operation result into a Minimal API response, using route metadata for successful create-style responses when supplied.
    /// </summary>
    /// <typeparam name="T">The type of content carried by the operation result.</typeparam>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="routeName">The route name used to build the location of the created resource.</param>
    /// <param name="routeValues">The route values used with <paramref name="routeName"/>.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; defaults depend on whether content, route metadata, or file content is present.</param>
    /// <returns>An <see cref="IResult"/> that represents the operation outcome.</returns>
    /// <remarks>
    /// Successful file content is converted to Minimal API file results so business services can return <see cref="StreamFileContent"/> or <see cref="ByteArrayFileContent"/> without referencing ASP.NET Core response types.
    /// </remarks>
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

        if (HasValidContent(content))
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

    private static bool HasValidContent(object? content)
        => content switch
        {
            null => false,
            JsonElement { ValueKind: JsonValueKind.Undefined } => false,
            _ => true
        };
}
