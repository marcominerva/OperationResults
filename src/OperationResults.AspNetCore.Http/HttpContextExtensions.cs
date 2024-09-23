using Microsoft.AspNetCore.Http;

namespace OperationResults.AspNetCore.Http;

public static class HttpContextExtensions
{
    public static IResult CreateResponse(this HttpContext httpContext, Result result, int? successStatusCode = null)
        => result.ToResponse(httpContext, successStatusCode);

    public static IResult CreateResponse(this HttpContext httpContext, Result result, string? routeName, object? routeValues = null)
        => result.ToResponse(httpContext, routeName, routeValues);

    public static IResult CreateResponse<T>(this HttpContext httpContext, Result<T> result, int? successStatusCode = null)
        => result.ToResponse(httpContext, null, null, successStatusCode);

    public static IResult CreateResponse<T>(this HttpContext httpContext, Result<T> result, string? routeName, object? routeValues = null, int? successStatusCode = null)
        => result.ToResponse(httpContext, routeName, routeValues, successStatusCode);
}
