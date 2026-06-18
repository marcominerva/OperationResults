using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OperationResults.AspNetCore.Http;

/// <summary>
/// Provides dependency-injection extensions for configuring operation-result response mapping in ASP.NET Core Minimal API applications.
/// </summary>
/// <remarks>
/// Registering these services centralizes how Minimal API endpoints translate <see cref="Result"/> and <see cref="Result{T}"/> failures into HTTP problem details while keeping business services free from ASP.NET Core response types.
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
    {
        var operationResultOptions = new OperationResultOptions();
        configuration?.Invoke(operationResultOptions);

        services.TryAddSingleton(operationResultOptions);

        return services;
    }
}
