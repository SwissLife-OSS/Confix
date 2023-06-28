using System.CommandLine.Builder;

namespace Confix.Tool.Middlewares;

public static class ConfigurationCommandlineBuilderExtensions
{
    public static CommandLineBuilder RegisterConfigurationMiddlewares(
        this CommandLineBuilder builder)
    {
        builder.AddTransient<LoadConfigurationMiddleware>();
        return builder;
    }
}
