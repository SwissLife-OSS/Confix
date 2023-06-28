using System.CommandLine.Builder;

namespace Confix.Tool.Middlewares;

public static class EnvironmentCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterEnvironmentMiddlewares(this CommandLineBuilder builder)
    {
        builder.AddTransient<EnvironmentMiddleware>();

        return builder;
    }
}
