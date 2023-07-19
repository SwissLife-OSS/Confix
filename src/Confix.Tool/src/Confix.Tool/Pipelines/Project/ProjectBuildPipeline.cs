using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectBuildPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseReadConfigurationFiles()
            .UseEnvironment()
            .UseBuildComponentsOfProject()
            .UseCompleteWhenNoConfigurationFiles()
            .Use<VariableMiddleware>()
            .Use<BuildProjectMiddleware>()
            .Use<ValidationMiddleware>()
            .UseWriteConfigurationFiles();
    }
}