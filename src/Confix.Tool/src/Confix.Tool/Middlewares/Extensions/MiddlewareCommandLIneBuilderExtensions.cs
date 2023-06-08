using System.CommandLine.Builder;
using Confix.Tool.Entities.Components;
using Confix.Tool.Middlewares.JsonSchemas;
using ConfiX.Variables;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class MiddlewareCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterMiddlewares(this CommandLineBuilder builder)
    {
        builder
            .AddSingleton<LoadConfigurationMiddleware>()
            .RegisterComponentInputs()
            .RegisterComponentProviders()
            .RegisterConfigurationAdapters()
            .RegisterJsonSchemaCollectionMiddleware()
            .RegisterVariableMiddleware();

        return builder;
    }
}
