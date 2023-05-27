using Microsoft.Extensions.DependencyInjection;

namespace Common.Components.DataProtection;

public static class DataProtectionServiceCollectionExtensions
{
    public static IServiceCollection AddDataProtectionFromConfig(this IServiceCollection services)
    {
        return services;
    }
}
