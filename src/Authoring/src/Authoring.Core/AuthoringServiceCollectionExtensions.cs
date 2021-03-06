using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Authoring
{
    public static class AuthoringServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthoringCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddScoped<IComponentService, ComponentService>();
            services.AddSingleton<IVariableService, VariableService>();
            services.AddSingleton<IVariableCryptoProvider, DoNotUseVariableCrypoProvider>();

            return services;
        }
    }
}
