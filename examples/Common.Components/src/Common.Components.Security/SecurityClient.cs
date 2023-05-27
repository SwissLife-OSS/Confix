using Microsoft.Extensions.DependencyInjection;

namespace Common.Components.Security;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        return services;
    }
}
