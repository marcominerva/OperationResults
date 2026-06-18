using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OperationResults.AspNetCore;

/// <summary>
/// Provides dependency-injection extensions for configuring operation-result response mapping in ASP.NET Core applications.
/// </summary>
/// <remarks>
/// Registering these services centralizes how controllers translate <see cref="Result"/> and <see cref="Result{T}"/> failures into HTTP problem details, including optional integration with ASP.NET Core automatic model-state validation responses.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers operation-result response mapping with optional configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">An optional delegate used to customize status-code mappings and validation error formatting.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so additional services can be chained.</returns>
    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration = null)
        => services.AddOperationResult(configuration, false, (modelState) => null);

    /// <summary>
    /// Registers operation-result response mapping and optionally replaces ASP.NET Core automatic invalid-model-state responses.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="updateModelStateResponseFactory">A value indicating whether to emit operation-result-style problem details for automatic model-state validation failures.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so additional services can be chained.</returns>
    public static IServiceCollection AddOperationResult(this IServiceCollection services, bool updateModelStateResponseFactory)
        => services.AddOperationResult(null, updateModelStateResponseFactory, (modelState) => null);

    /// <summary>
    /// Registers operation-result response mapping with options and optional automatic model-state response integration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">An optional delegate used to customize status-code mappings and validation error formatting.</param>
    /// <param name="updateModelStateResponseFactory">A value indicating whether to emit operation-result-style problem details for automatic model-state validation failures.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so additional services can be chained.</returns>
    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration, bool updateModelStateResponseFactory)
        => services.AddOperationResult(configuration, updateModelStateResponseFactory, (modelState) => null);

    /// <summary>
    /// Registers operation-result response mapping and configures a fixed title for automatic model-state validation problem responses.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="validationErrorDefaultMessage">The message used as the problem details title for automatic model-state validation failures.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so additional services can be chained.</returns>
    public static IServiceCollection AddOperationResult(this IServiceCollection services, string? validationErrorDefaultMessage)
        => services.AddOperationResult(null, true, (modelState) => validationErrorDefaultMessage);

    /// <summary>
    /// Registers operation-result response mapping with options and a fixed title for automatic model-state validation problem responses.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">An optional delegate used to customize status-code mappings and validation error formatting.</param>
    /// <param name="validationErrorDefaultMessage">The message used as the problem details title for automatic model-state validation failures.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so additional services can be chained.</returns>
    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration, string? validationErrorDefaultMessage)
        => services.AddOperationResult(configuration, true, (modelState) => validationErrorDefaultMessage);

    /// <summary>
    /// Registers operation-result response mapping and configures a dynamic title for automatic model-state validation problem responses.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="validationErrorMessageProvider">A delegate that receives the current <see cref="ModelStateDictionary"/> and returns the problem details title for automatic validation failures.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so additional services can be chained.</returns>
    public static IServiceCollection AddOperationResult(this IServiceCollection services, Func<ModelStateDictionary, string?>? validationErrorMessageProvider)
        => services.AddOperationResult(null, true, validationErrorMessageProvider);

    /// <summary>
    /// Registers operation-result response mapping with options and a dynamic title for automatic model-state validation problem responses.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">An optional delegate used to customize status-code mappings and validation error formatting.</param>
    /// <param name="validationErrorMessageProvider">A delegate that receives the current <see cref="ModelStateDictionary"/> and returns the problem details title for automatic validation failures.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so additional services can be chained.</returns>
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

                    var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext, statusCode, validationErrorMessageProvider?.Invoke(actionContext.ModelState) ?? "One or more validation errors occurred",
                        instance: httpContext.Request.Path);
                    problemDetails.Type ??= $"https://httpstatuses.io/{statusCode}";

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

                    var problemDetailsResult = new JsonResult(problemDetails)
                    {
                        StatusCode = statusCode,
                        ContentType = "application/problem+json"
                    };

                    return problemDetailsResult;
                };
            });
        }

        return services;
    }
}
