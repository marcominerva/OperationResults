using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        return result switch
        {
            { Success: true } => TypedResults.StatusCode(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent)),
            _ => Problem(httpContext, result is Error error ? error : null)
        };
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
        return result switch
        {
            { Success: true } => TypedResults.CreatedAtRoute(routeName, routeValues),
            _ => Problem(httpContext, result is Error error ? error : null)
        };
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
        return result switch
        {
            { Success: true, Content: not null } when !string.IsNullOrWhiteSpace(routeName) => TypedResults.CreatedAtRoute(result.Content, routeName, routeValues),
            { Success: true, Content: StreamFileContent streamFileContent } => TypedResults.Stream(streamFileContent.Content, streamFileContent.ContentType, streamFileContent.DownloadFileName),
            { Success: true, Content: ByteArrayFileContent byteArrayFileContent } => TypedResults.File(byteArrayFileContent.Content, byteArrayFileContent.ContentType, byteArrayFileContent.DownloadFileName),
            { Success: true, Content: not null } => TypedResults.Json(result.Content, statusCode: successStatusCode.GetValueOrDefault(StatusCodes.Status200OK)),
            { Success: true } => TypedResults.StatusCode(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent)),
            _ => Problem(httpContext, result is Error error ? error : null)
        };
    }

    private static ProblemHttpResult Problem(HttpContext httpContext, Error? error)
    {
        var options = httpContext.RequestServices.GetService<OperationResultOptions>() ?? new OperationResultOptions();

        // A failed result without an Error comes from Result(false) (which is also the default Result) or from a default Result<T>: it has no reason, so it is treated as a generic failure.
        var failureReason = error?.FailureReason ?? FailureReasons.GenericError;
        var statusCode = options.MapStatusCodes ? options.GetStatusCode(failureReason) : failureReason;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error?.ErrorMessage ?? ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = error?.ErrorDetail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);

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

        var problemResult = TypedResults.Problem(problemDetails);
        problemResult.ProblemDetails.Type ??= $"https://httpstatuses.io/{statusCode}";

        return problemResult;
    }
}
