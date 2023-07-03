using System.CommandLine.Builder;
using Confix.Tool.Entities.Components;
using Confix.Tool.Middlewares.JsonSchemas;
using Confix.Tool.Middlewares.Project.Extensions;

namespace Confix.Tool.Middlewares;

public static class MiddlewareCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterMiddlewares(this CommandLineBuilder builder)
    {
        builder
            .AddTransient<LoadConfigurationMiddleware>()
            .AddTransient<EnvironmentMiddleware>()
            .RegisterProjectMiddlewares()
            .RegisterComponentInputs()
            .RegisterConfigurationFiles()
            .RegisterComponentProviders()
            .RegisterConfigurationAdapters()
            .RegisterJsonSchemaCollectionMiddleware()
            .RegisterVariableMiddleware();

        return builder;
    }
}
