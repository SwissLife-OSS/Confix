using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class ConfigurationAdapterCommandLineBuilderExtensions
{
    public static ConfixCommandLineBuilder RegisterConfigurationAdapters(this ConfixCommandLineBuilder builder)
    {
        builder.AddSingleton(sp => new ConfigurationAdapterMiddleware(
            sp.GetRequiredService<IEnumerable<IConfigurationAdapter>>()));

        builder.AddConfigurationAdapter<VsCodeConfigurationAdapter>();
        builder.AddConfigurationAdapter<IntelliJConfigurationAdapter>();

        return builder;
    }
}
