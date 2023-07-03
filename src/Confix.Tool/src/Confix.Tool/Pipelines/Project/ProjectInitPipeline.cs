using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Commands.Solution;

public sealed class ProjectInitPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .Use<InitProjectMiddleware>();
    }
}
