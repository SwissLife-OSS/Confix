using System.CommandLine.Builder;
using Confix.Tool.Entities.Components;
using Confix.Tool.Middlewares.JsonSchemas;
using Confix.Tool.Middlewares.Project;
using ConfiX.Variables;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class MiddlewareCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterMiddlewares(this CommandLineBuilder builder)
    {
        builder
            .AddTransient(sp => new BuildComponentsOfProjectMiddleware(sp))
            .AddTransient<LoadConfigurationMiddleware>()
            .AddTransient<EnvironmentMiddleware>()
            .AddTransient<BuildProjectMiddleware>()
            .RegisterComponentInputs()
            .RegisterConfigurationFiles()
            .RegisterComponentProviders()
            .RegisterConfigurationAdapters()
            .RegisterJsonSchemaCollectionMiddleware()
            .RegisterVariableMiddleware();

        return builder;
    }
}
