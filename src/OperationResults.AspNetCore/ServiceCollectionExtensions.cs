using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OperationResults.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration = null)
        => services.AddOperationResult(configuration, false, (modelState) => null);

    public static IServiceCollection AddOperationResult(this IServiceCollection services, bool updateModelStateResponseFactory)
        => services.AddOperationResult(null, updateModelStateResponseFactory, (modelState) => null);

    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration, bool updateModelStateResponseFactory)
        => services.AddOperationResult(configuration, updateModelStateResponseFactory, (modelState) => null);

    public static IServiceCollection AddOperationResult(this IServiceCollection services, string? validationErrorDefaultMessage)
        => services.AddOperationResult(null, true, (modelState) => validationErrorDefaultMessage);

    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration, string? validationErrorDefaultMessage)
        => services.AddOperationResult(configuration, true, (modelState) => validationErrorDefaultMessage);

    public static IServiceCollection AddOperationResult(this IServiceCollection services, Func<ModelStateDictionary, string?>? validationErrorMessageProvider)
        => services.AddOperationResult(null, true, validationErrorMessageProvider);

    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration, Func<ModelStateDictionary, string?>? validationErrorMessageProvider)
        => services.AddOperationResult(configuration, true, validationErrorMessageProvider);

    private static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration, bool updateModelStateResponseFactory, Func<ModelStateDictionary, string?>? validationErrorMessageProvider)
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
                    var statusCode = operationResultOptions.GetStatusCode(FailureReasons.ClientError, StatusCodes.Status400BadRequest);
                    var problemDetails = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = validationErrorMessageProvider?.Invoke(actionContext.ModelState) ?? "One or more validation errors occurred",
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
