using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OperationResults.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration = null)
    {
        var options = new OperationResultOptions();
        configuration?.Invoke(options);

        services.TryAddSingleton(options);

        return services;
    }
}
