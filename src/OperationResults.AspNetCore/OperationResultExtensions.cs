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
        if (result.Success)
        {
            return new StatusCodeResult(successStatusCode.GetValueOrDefault(StatusCodes.Status204NoContent));
        }

        return Problem(httpContext, result.FailureReason, null, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
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
        if (result.Success)
        {
            var createdAtRouteResult = new CreatedAtRouteResult(routeName, routeValues, null)
            {
                StatusCode = successStatusCode.GetValueOrDefault(StatusCodes.Status201Created)
            };

            return createdAtRouteResult;
        }

        return Problem(httpContext, result.FailureReason, null, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
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

        return Problem(httpContext, result.FailureReason, result.Content, result.ErrorMessage, result.ErrorDetail, result.ValidationErrors);
    }

    private static IActionResult Problem(HttpContext httpContext, int failureReason, object? content = null, string? title = null, string? detail = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        var options = httpContext.RequestServices.GetService<OperationResultOptions>() ?? new OperationResultOptions();
        var statusCode = options.MapStatusCodes ? options.GetStatusCode(failureReason) : failureReason;

        if (content is not null)
        {
            var objectResult = new ObjectResult(content)
            {
                StatusCode = statusCode
            };

            return objectResult;
        }

        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext, statusCode, title ?? ReasonPhrases.GetReasonPhrase(statusCode),
            detail: detail, instance: httpContext.Request.Path);
        problemDetails.Type ??= $"https://httpstatuses.io/{statusCode}";

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

        var problemDetailsResult = new JsonResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentType = "application/problem+json"
        };

        return problemDetailsResult;
    }
}
