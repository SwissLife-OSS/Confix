using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands;

public sealed class ValidateCommandPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseEnvironment()
            .UseHandler<IServiceProvider>(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context, IServiceProvider services)
    {
        context.SetStatus("Validating...");

        var configuration = context.Features.Get<ConfigurationFeature>();

        switch (configuration.Scope)
        {
            case ConfigurationScope.None:
                context.Logger
                    .LogNoConfixContextWasFound(context.Execution.CurrentDirectory.FullName);
                throw new ExitException();

            case ConfigurationScope.Component:
                App.Log.ComponentsDoNotSupportValidate();
                throw new ExitException();

            case ConfigurationScope.Project:
                {
                    var projectDirectory = configuration.Project!.Directory!;
                    var pipeline = new ProjectValidatePipeline();
                    var projectContext = context
                        .WithExecutingDirectory(projectDirectory)
                        .WithFeatureCollection();

                    await pipeline.ExecuteAsync(services, projectContext);
                    return;
                }

            case ConfigurationScope.Solution:
                {
                    var solutionDirectory = configuration.Solution!.Directory!;
                    var pipeline = new SolutionValidatePipeline();
                    var solutionContext = context
                        .WithExecutingDirectory(solutionDirectory)
                        .WithFeatureCollection();

                    await pipeline.ExecuteAsync(services, solutionContext);
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

    public static void ComponentsDoNotSupportValidate(this IConsoleLogger console)
    {
        console.Error(
            "Components do not support reload. `reload` only works for projects and solutions.");
    }
}
