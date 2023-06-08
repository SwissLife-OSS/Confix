using System.CommandLine.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class ConfigurationAdapterCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterConfigurationAdapters(this CommandLineBuilder builder)
    {
        builder.AddSingleton(sp => new ConfigurationAdapterMiddleware(
            sp.GetRequiredService<IEnumerable<IConfigurationAdapter>>()));
        
        builder.AddConfigurationAdapter<VsCodeConfigurationAdapter>();

        return builder;
    }
}

