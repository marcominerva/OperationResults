using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OperationResults.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration = null, bool updateModelStateResponseFactory = false, string? validationErrorDefaultMessage = null)
    {
        var operationResultOptions = new OperationResultOptions();
        configuration?.Invoke(operationResultOptions);

        services.TryAddSingleton(operationResultOptions);

        if (updateModelStateResponseFactory)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var httpContext = actionContext.HttpContext;
                    var statusCode = operationResultOptions.GetStatusCode(FailureReasons.ClientError);
                    var problemDetails = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = validationErrorDefaultMessage ?? "One or mode validation errors occurred",
                        Type = $"https://httpstatuses.io/{statusCode}",
                        Instance = httpContext.Request.Path
                    };

                    problemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);

                    if (operationResultOptions.ErrorResponseFormat == ErrorResponseFormat.Default)
                    {
                        var errors = actionContext.ModelState
                            .Where(e => e.Value?.Errors.Any() ?? false)
                            .ToDictionary(k => k.Key, v => v.Value!.Errors.Select(e => e.ErrorMessage));

                        problemDetails.Extensions.Add("errors", errors);
                    }
                    else
                    {
                        var errors = actionContext.ModelState
                            .Where(e => e.Value?.Errors.Any() ?? false)
                            .SelectMany(e => e.Value!.Errors.Select(v => new ValidationError(e.Key, v.ErrorMessage)));

                        problemDetails.Extensions.Add("errors", errors);
                    }

                    var result = new ObjectResult(problemDetails)
                    {
                        StatusCode = statusCode
                    };

                    return result;
                };
            });
        }

        return services;
    }
}
