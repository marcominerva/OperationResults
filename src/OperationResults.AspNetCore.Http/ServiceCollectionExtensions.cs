using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OperationResults.AspNetCore.Http;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOperationResult(this IServiceCollection services, Action<OperationResultOptions>? configuration = null)
    {
        var operationResultOptions = new OperationResultOptions();
        configuration?.Invoke(operationResultOptions);

        services.TryAddSingleton(operationResultOptions);

        return services;
    }
}
