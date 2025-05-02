using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.JsonSchemas;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectBuildPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddOption(NoRestoreOptions.Instance)
            .AddOption(GitUsernameOptions.Instance)
            .AddOption(GitTokenOptions.Instance)
            .Use<LoadConfigurationMiddleware>()
            .UseReadConfigurationFiles()
            .UseEnvironment()
            .UseBuildComponentsOfProject()
            .UseCompleteWhenNoConfigurationFiles()
            .Use<VariableMiddleware>()
            .When(x =>
                    !x.Parameter.TryGet(NoRestoreOptions.Instance, out bool noRestore) || noRestore,
                n => n
                    .Use<JsonSchemaCollectionMiddleware>()
                    .Use<ConfigurationAdapterMiddleware>()
                    .Use<BuildComponentProviderMiddleware>()
                    .Use<RestoreProjectMiddleware>())
            .Use<InitializeConfigurationDefaultValues>()
            .Use<BuildProjectMiddleware>()
            .Use<ValidationMiddleware>()
            .UseWriteConfigurationFiles();
    }
}
