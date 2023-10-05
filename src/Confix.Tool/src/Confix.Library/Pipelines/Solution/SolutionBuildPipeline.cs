using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;

namespace Confix.Tool.Commands.Solution;

public sealed class SolutionBuildPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Building the schema of the solution...");

        var configuration = context.Features.Get<ConfigurationFeature>();
        var solution = configuration.EnsureSolution();

        var components = solution.Directory!.FindAllInPath(FileNames.ConfixComponent);

        // we build the components first, so that we make sure all embedded resources are up to date
        // before we build the projects
        context.Status.Message = "Building components...";
        foreach (var component in components)
        {
            var componentDirectory = component.Directory!;
            var pipeline = new ComponentBuildPipeline();
            var componentContext = context
                .WithExecutingDirectory(componentDirectory)
                .WithFeatureCollection();

            await pipeline.ExecuteAsync(componentContext);
        }

        var projects = solution.Directory!.FindAllInPath(FileNames.ConfixProject);

        foreach (var project in projects)
        {
            context.Logger.LogProjectedDetected(project);

            var projectDirectory = project.Directory!;
            var pipeline = new ProjectBuildPipeline();
            var projectContext = context
                .WithExecutingDirectory(projectDirectory)
                .WithFeatureCollection();

            await pipeline.ExecuteAsync(projectContext);
        }
    }
}

file static class Log
{
    public static void LogProjectedDetected(
        this IConsoleLogger console,
        FileInfo info)
    {
        console.Information(
            $"Project detected: {info.Directory?.Name.ToLink(info)} [dim]{info.FullName}[/]");
    }
}
