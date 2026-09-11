using Confix.Tool.Commands.Logging;
using Confix.Tool.Entities.Components;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Middlewares.Encryption;
using Confix.Tool.Middlewares.JsonSchemas;
using Confix.Tool.Middlewares.Project;
using Confix.Tool.Reporting;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class MiddlewareCommandLineBuilderExtensions
{
    public static ConfixCommandLineBuilder RegisterMiddlewares(this ConfixCommandLineBuilder builder)
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
            .RegisterDependencyAnalyzerMiddleware()
            .RegisterVariableMiddleware()
            .RegisterDependencyProviders()
            .RegisterEncryptionMiddleware();

        return builder;
    }
}
