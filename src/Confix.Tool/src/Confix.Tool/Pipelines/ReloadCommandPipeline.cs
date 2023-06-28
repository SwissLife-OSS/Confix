using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands;

public sealed class ReloadCommandPipeline : Pipeline
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
        context.SetStatus("Reloading the schema of the project...");

        var configuration = context.Features.Get<ConfigurationFeature>();

        switch (configuration.Scope)
        {
            case ConfigurationScope.None:
                context.Logger
                    .LogNoConfixContextWasFound(context.Execution.CurrentDirectory.FullName);
                throw new ExitException();

            case ConfigurationScope.Component:
                App.Log.ComponentsDoNotSupportReload();
                throw new ExitException();

            case ConfigurationScope.Project:
                {
                    var projectDirectory = configuration.Project!.Directory!;
                    var pipeline = new ProjectReloadPipeline();
                    var projectContext = context
                        .WithExecutingDirectory(projectDirectory)
                        .WithFeatureCollection();

                    await pipeline.ExecuteAsync(services, projectContext);
                    return;
                }

            case ConfigurationScope.Solution:
                {
                    var solutionDirectory = configuration.Solution!.Directory!;
                    var pipeline = new SolutionReloadPipeline();
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
            $"No confix context was found in the executing directory: {directory.AsPath()}");
    }

    public static void ComponentsDoNotSupportReload(this IConsoleLogger console)
    {
        console.Error(
            "Components do not support reload. `reload` only works for projects and solutions.");
    }
}
