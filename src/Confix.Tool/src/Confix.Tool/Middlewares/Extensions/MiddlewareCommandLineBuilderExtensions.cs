using System.CommandLine.Builder;
using Confix.Tool.Entities.Components;
using Confix.Tool.Middlewares.Artifact;
using Confix.Tool.Middlewares.JsonSchemas;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Middlewares;

public static class MiddlewareCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterMiddlewares(this CommandLineBuilder builder)
    {
        builder
            .RegisterConfigurationMiddlewares()
            .RegisterEnvironmentMiddlewares()
            .RegisterProjectMiddlewares()
            .RegisterArtifactFile()
            .RegisterComponentInputs()
            .RegisterConfigurationFiles()
            .RegisterComponentProviders()
            .RegisterConfigurationAdapters()
            .RegisterJsonSchemaCollectionMiddleware()
            .RegisterVariableMiddleware();

        return builder;
    }
}
