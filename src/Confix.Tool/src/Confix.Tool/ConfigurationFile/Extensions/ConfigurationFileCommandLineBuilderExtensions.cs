using System.CommandLine.Builder;
using Confix.Tool.Entities.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFileCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterConfigurationFiles(this CommandLineBuilder builder)
    {
        builder.AddSingleton(sp => new ConfigurationFileMiddleware(
            sp.GetRequiredService<IConfigurationFileProviderFactory>()));

        builder.AddConfigurationFileProvider<InlineConfigurationFileProvider>();

        return builder;
    }
}