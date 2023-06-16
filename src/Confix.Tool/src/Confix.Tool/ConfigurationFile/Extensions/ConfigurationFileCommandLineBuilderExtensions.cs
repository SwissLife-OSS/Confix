using System.CommandLine.Builder;
using Confix.Tool.Entities.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFileCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterConfigurationFiles(this CommandLineBuilder builder)
    {
        builder.AddSingleton<WriteConfigurationFileMiddleware>();
        builder.AddSingleton(sp => new ReadConfigurationFileMiddleware(
            sp.GetRequiredService<IConfigurationFileProviderFactory>()));

        builder.AddConfigurationFileProvider<InlineConfigurationFileProvider>();

        return builder;
    }
}
