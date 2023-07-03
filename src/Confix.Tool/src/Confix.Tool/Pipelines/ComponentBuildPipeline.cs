using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands;

public class ComponentBuildPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .AddOption(ActiveEnvironmentOption.Instance)
            .AddOption(OutputFileOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Building the schema of the project...");

        var configuration = context.Features.Get<ConfigurationFeature>();

        switch (configuration.Scope)
        {
            case ConfigurationScope.None:
                context.Logger
                    .LogNoConfixContextWasFound(context.Execution.CurrentDirectory.FullName);
                throw new ExitException();

            case ConfigurationScope.Component:
            {
                var projectDirectory = configuration.Project!.Directory!;
                var pipeline = new ComponentBuildPipeline();
                var projectContext = context
                    .WithExecutingDirectory(projectDirectory);

                await pipeline.ExecuteAsync(projectContext);
                return;
            }

            case ConfigurationScope.Project:
            {
                var projectDirectory = configuration.Project!.Directory!;
                var pipeline = new ProjectBuildPipeline();
                var projectContext = context
                    .WithExecutingDirectory(projectDirectory);

                await pipeline.ExecuteAsync(projectContext);
                return;
            }

            case ConfigurationScope.Solution:
            {
                var solutionDirectory = configuration.Solution!.Directory!;
                var pipeline = new SolutionBuildPipeline();
                var solutionContext = context
                    .WithExecutingDirectory(solutionDirectory);

                await pipeline.ExecuteAsync(solutionContext);
                return;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

file static class Log
{
    public static void LogNoConfixContextWasFound(
        this IConsoleLogger console,
        string directory)
    {
        console.Error(
            $"No confix context was found in the executing directory: [yellow]{directory}[/]");
    }
}
