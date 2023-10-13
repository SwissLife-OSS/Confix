using Confix.Tool.Commands.Project;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Reporting;

public sealed class ProjectReportPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddOption(NoRestoreOptions.Instance)
            .AddOption(ActiveEnvironmentOption.Instance)
            .AddOption(ReportOutputFileOption.Instance)
            .Use<LoadConfigurationMiddleware>()
            .AddContextData(Context.DisableConfigurationWrite, true)
            .Use(BuildProject)
            .Use<ProjectReportMiddleware>();
    }

    /// <summary>
    /// Executes the whole build pipeline for a project. This way components are loaded and
    /// variables are resolved.
    /// </summary>
    private static async Task BuildProject(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();
        var project = configuration.EnsureProject();

        var projectDirectory = project.Directory!;
        var pipeline = new ProjectBuildPipeline();

        var projectContext = context
            .WithExecutingDirectory(projectDirectory);

        await pipeline.ExecuteAsync(projectContext);

        await next(context);
    }
}
