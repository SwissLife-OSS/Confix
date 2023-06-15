using System.CommandLine.Invocation;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Project;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using ExecutionContext = Confix.Tool.Common.Pipelines.ExecutionContext;

namespace Confix.Tool.Commands;

public class ReloadCommandPipeline : Pipeline
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
        context.SetStatus("Reloading the schema of the project...");

        var configuration = context.Features.Get<ConfigurationFeature>();

        switch (configuration.Scope)
        {
            case ConfigurationScope.None:
                context.Logger
                    .LogNoConfixContextWasFound(context.Execution.CurrentDirectory.FullName);
                break;

            case ConfigurationScope.Component:
                break;

            case ConfigurationScope.Project:
                var projectDirectory = configuration.Project!.Directory!;
                var pipeline = new ProjectReloadPipeline();
                var projectContext = context.WithExecutingDirectory(projectDirectory);

                await pipeline.ExecuteAsync(services, projectContext);

                break;

            case ConfigurationScope.Solution:
                break;

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

file static class Extensions
{
    public static MiddlewareContext WithExecutingDirectory(
        this IMiddlewareContext context,
        DirectoryInfo project)

    {
        return (MiddlewareContext) context with
        {
            Execution = (ExecutionContext) context.Execution with
            {
                CurrentDirectory = project
            }
        };
    }
}
