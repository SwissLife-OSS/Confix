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
            .UseHandler<IServiceProvider>(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context, IServiceProvider services)
    {
        context.SetStatus("Building the schema of the solution...");

        var configuration = context.Features.Get<ConfigurationFeature>();
        var solution = configuration.EnsureSolution();

        var projects = solution.Directory!.FindAllInPath(FileNames.ConfixProject);

        foreach (var project in projects)
        {
            context.Logger.LogProjectedDetected(project);

            var projectDirectory = project.Directory!;
            var pipeline = new ProjectBuildPipeline();
            var projectContext = context
                .WithExecutingDirectory(projectDirectory)
                .WithFeatureCollection();

            await pipeline.ExecuteAsync(services, projectContext);
        }
    }
}

file static class Log
{
    public static void LogProjectedDetected(
        this IConsoleLogger console,
        FileInfo project)
    {
        console.Information($"Project detected: [grey]{project.FullName}[/]");
    }
}
