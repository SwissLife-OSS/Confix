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
            .UseConfigurationFiles()
            .UseEnvironment()
            .Use<BuildComponentsOfProjectMiddleware>()
            .Use<VariableMiddleware>()
            .Use<BuildProjectMiddleware>()
            .Use<ValidationMiddleware>();
    }
}
