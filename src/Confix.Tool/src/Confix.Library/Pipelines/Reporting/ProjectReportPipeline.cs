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
            .AddOption(ReportInputFileOption.Instance)
            .Use<LoadConfigurationMiddleware>()
            .AddContextData(Context.DisableConfigurationWrite, true)
            .When(x => !x.Parameter.HasInputFile(), x => x.Use(BuildProject))
            .When(x => x.Parameter.HasInputFile(),
                x => x.UseReadConfigurationFiles()
                    .UseEnvironment()
                    .UseBuildComponentsOfProject()
                    .Use<VariableMiddleware>())
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

file static class Extensions
{
    public static bool HasInputFile(this IParameterCollection collection)
        => collection.TryGet(ReportInputFileOption.Instance, out FileInfo file) && file.Exists;
}
