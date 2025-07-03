using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectEnvironmentsPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static Task InvokeAsync(IMiddlewareContext context)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();
        var project = configuration.EnsureProject();
        var environments = project.Environments;
        
        foreach (var environment in environments)
        {
            context.Logger.Always(environment.Name);
        }

        return Task.CompletedTask;
    }
}
