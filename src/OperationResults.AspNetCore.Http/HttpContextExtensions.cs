using Microsoft.AspNetCore.Http;

namespace OperationResults.AspNetCore.Http;

/// <summary>
/// Provides <see cref="HttpContext"/> extension methods that turn operation results into ASP.NET Core Minimal API responses.
/// </summary>
/// <remarks>
/// These helpers keep endpoint delegates concise while preserving the separation between business services that return <see cref="Result"/> values and Minimal API code that returns <see cref="IResult"/>.
/// </remarks>
public static class HttpContextExtensions
{
    /// <summary>
    /// Creates a Minimal API response for a non-generic operation result.
    /// </summary>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; the default is <see cref="StatusCodes.Status204NoContent"/>.</param>
    /// <returns>An <see cref="IResult"/> that represents the operation outcome.</returns>
    public static IResult CreateResponse(this HttpContext httpContext, Result result, int? successStatusCode = null)
        => result.ToResponse(httpContext, successStatusCode);

    /// <summary>
    /// Creates a Minimal API route-based response for a successful non-generic operation result, or a problem response when it fails.
    /// </summary>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="routeName">The route name used to build the location of the created resource.</param>
    /// <param name="routeValues">The route values used with <paramref name="routeName"/>.</param>
    /// <remarks>The success response uses <see cref="StatusCodes.Status201Created"/>.</remarks>
    /// <returns>An <see cref="IResult"/> that represents the operation outcome.</returns>
    public static IResult CreateResponse(this HttpContext httpContext, Result result, string? routeName, object? routeValues = null)
        => result.ToResponse(httpContext, routeName, routeValues);

    /// <summary>
    /// Creates a Minimal API response for a typed operation result.
    /// </summary>
    /// <typeparam name="T">The type of content carried by the operation result.</typeparam>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; the default is <see cref="StatusCodes.Status200OK"/> when content exists or <see cref="StatusCodes.Status204NoContent"/> when it does not.</param>
    /// <returns>An <see cref="IResult"/> that represents the operation outcome.</returns>
    public static IResult CreateResponse<T>(this HttpContext httpContext, Result<T> result, int? successStatusCode = null)
        => result.ToResponse(httpContext, null, null, successStatusCode);

    /// <summary>
    /// Creates a Minimal API route-based response for a successful typed operation result, or a problem response when it fails.
    /// </summary>
    /// <typeparam name="T">The type of content carried by the operation result.</typeparam>
    /// <param name="httpContext">The current HTTP context used to resolve response-mapping services and request metadata.</param>
    /// <param name="result">The operation result to translate.</param>
    /// <param name="routeName">The route name used to build the location of the created resource.</param>
    /// <param name="routeValues">The route values used with <paramref name="routeName"/>.</param>
    /// <param name="successStatusCode">The status code to use when <paramref name="result"/> succeeds; the default is <see cref="StatusCodes.Status201Created"/> when <paramref name="routeName"/> is provided.</param>
    /// <returns>An <see cref="IResult"/> that represents the operation outcome.</returns>
    public static IResult CreateResponse<T>(this HttpContext httpContext, Result<T> result, string? routeName, object? routeValues = null, int? successStatusCode = null)
        => result.ToResponse(httpContext, routeName, routeValues, successStatusCode);
}
