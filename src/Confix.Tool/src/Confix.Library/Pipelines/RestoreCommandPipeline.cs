using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands;

public sealed class RestoreCommandPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Reloading the schema of the project...");

        var configuration = context.Features.Get<ConfigurationFeature>();

        switch (configuration.Scope)
        {
            case ConfigurationScope.None:
                var directory = context.Execution.CurrentDirectory.FullName;
                throw new ExitException(
                    $"No confix context was found in the executing directory: [yellow]{directory}[/]");

            case ConfigurationScope.Component:
                throw new ExitException(
                    "Components do not support reload. `reload` only works for projects and solutions.");

            case ConfigurationScope.Project:
            {
                var projectDirectory = configuration.Project!.Directory!;
                var pipeline = new ProjectRestorePipeline();
                var projectContext = context
                    .WithExecutingDirectory(projectDirectory)
                    .WithFeatureCollection();

                await pipeline.ExecuteAsync(projectContext);
                return;
            }

            case ConfigurationScope.Solution:
            {
                var solutionDirectory = configuration.Solution!.Directory!;
                var pipeline = new SolutionRestorePipeline();
                var solutionContext = context
                    .WithExecutingDirectory(solutionDirectory)
                    .WithFeatureCollection();

                await pipeline.ExecuteAsync(solutionContext);
                return;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
