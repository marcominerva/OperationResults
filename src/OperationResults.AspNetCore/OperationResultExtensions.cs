using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace OperationResults.AspNetCore;

/// <summary>
/// Provides extension methods that convert operation results into ASP.NET Core <see cref="IActionResult"/> instances.
/// </summary>
/// <remarks>
/// These methods are the controller-based adapter between transport-neutral business results and HTTP responses. They centralize status-code mapping, problem details creation, validation error formatting, and file-result handling so controller actions can stay focused on orchestration.
/// </remarks>
public static class OperationResultExtensions
{
    /// <summary>
    /// Converts a non-generic operation result into an ASP.NET Core response.
    /// </summary>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; the default is <see cref="StatusCodes.Status204NoContent"/>.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the operation outcome.</returns>
    public static IActionResult ToResponse(this Result result, HttpContext httpContext, int? successStatusCode = null)
    {
        return result switch
        {
            { Success: true } => new StatusCodeResult(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent)),
            _ => Problem(httpContext, result is Error error ? error : null)
        };
    }

    /// <summary>
    /// Converts a non-generic operation result into an ASP.NET Core route-based response when it succeeds, or a problem response when it fails.
    /// </summary>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="routeName">The route name used to build the location of the created resource.</param>
    /// <param name="routeValues">The route values used with <paramref name="routeName"/>.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; the default is <see cref="StatusCodes.Status201Created"/>.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the operation outcome.</returns>
    public static IActionResult ToResponse(this Result result, HttpContext httpContext, string? routeName, object? routeValues = null, int? successStatusCode = null)
    {
        return result switch
        {
            { Success: true } => new CreatedAtRouteResult(routeName, routeValues, null)
            {
                StatusCode = successStatusCode.GetValueOrDefault(StatusCodes.Status201Created)
            },
            _ => Problem(httpContext, result is Error error ? error : null)
        };
    }

    /// <summary>
    /// Converts a typed operation result into an ASP.NET Core response.
    /// </summary>
    /// <typeparam name="T">The type of content carried by the operation result.</typeparam>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; the default is <see cref="StatusCodes.Status200OK"/> when content exists or <see cref="StatusCodes.Status204NoContent"/> when it does not.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the operation outcome.</returns>
    public static IActionResult ToResponse<T>(this Result<T> result, HttpContext httpContext, int? successStatusCode = null)
        => result.ToResponse(httpContext, null, null, successStatusCode);

    /// <summary>
    /// Converts a typed operation result into an ASP.NET Core response, using route metadata for successful create-style responses when supplied.
    /// </summary>
    /// <typeparam name="T">The type of content carried by the operation result.</typeparam>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="routeName">The route name used to build the location of the created resource.</param>
    /// <param name="routeValues">The route values used with <paramref name="routeName"/>.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; defaults depend on whether content, route metadata, or file content is present.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the operation outcome.</returns>
    /// <remarks>
    /// Successful file content is converted to <see cref="FileStreamResult"/> or <see cref="FileContentResult"/> so business services can return <see cref="StreamFileContent"/> or <see cref="ByteArrayFileContent"/> without referencing ASP.NET Core MVC types.
    /// </remarks>
    public static IActionResult ToResponse<T>(this Result<T> result, HttpContext httpContext, string? routeName, object? routeValues = null, int? successStatusCode = null)
    {
        return result switch
        {
            { Success: true, Content: not null } when !string.IsNullOrWhiteSpace(routeName) => new CreatedAtRouteResult(routeName, routeValues, result.Content)
            {
                StatusCode = successStatusCode.GetValueOrDefault(StatusCodes.Status201Created)
            },
            { Success: true, Content: StreamFileContent streamFileContent } => new FileStreamResult(streamFileContent.Content, streamFileContent.ContentType)
            {
                FileDownloadName = streamFileContent.DownloadFileName,
            },
            { Success: true, Content: ByteArrayFileContent byteArrayFileContent } => new FileContentResult(byteArrayFileContent.Content, byteArrayFileContent.ContentType)
            {
                FileDownloadName = byteArrayFileContent.DownloadFileName
            },
            { Success: true, Content: not null } => new ObjectResult(result.Content)
            {
                StatusCode = successStatusCode.GetValueOrDefault(StatusCodes.Status200OK)
            },
            { Success: true } => new StatusCodeResult(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent)),
            _ => Problem(httpContext, result is Error error ? error : null)
        };
    }

    private static JsonResult Problem(HttpContext httpContext, Error? error)
    {
        var options = httpContext.RequestServices.GetService<OperationResultOptions>() ?? new OperationResultOptions();

        // A failed result without an Error comes from Result(false) (which is also the default Result) or from a default Result<T>: it has no reason, so it is treated as a generic failure.
        // This synthetic reason is not an HTTP status code, so it is always mapped, even when MapStatusCodes is disabled.
        var failureReason = error?.FailureReason ?? FailureReasons.GenericError;
        var statusCode = error is null || options.MapStatusCodes ? options.GetStatusCode(failureReason) : failureReason;

        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext, statusCode, error?.ErrorMessage ?? ReasonPhrases.GetReasonPhrase(statusCode), detail: error?.ErrorDetail, instance: httpContext.Request.Path);
        problemDetails.Type ??= $"https://httpstatuses.io/{statusCode}";

        if (error?.ValidationErrors is { } validationErrors && validationErrors.Any())
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

        var problemDetailsResult = new JsonResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentType = MediaTypeNames.Application.ProblemJson
        };

        return problemDetailsResult;
    }
}
