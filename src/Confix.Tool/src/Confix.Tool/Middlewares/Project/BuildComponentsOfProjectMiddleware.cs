using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares.Project;

public sealed class BuildComponentsOfProjectMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.SetStatus("Building the components of a project");

        var configuration = context.Features.Get<ConfigurationFeature>();
        configuration.EnsureProjectScope();
        var project = configuration.EnsureProject();

        var files = project.Directory!.FindAllInPath(FileNames.ConfixComponent);

        foreach (var component in files)
        {
            context.Logger.LogComponentDetected(component);

            var componentDirectory = component.Directory!;
            var pipeline = new BuildComponentPipeline();
            var componentContext = context
                .WithExecutingDirectory(componentDirectory)
                .WithFeatureCollection();

            await pipeline.ExecuteAsync(componentContext);
        }

        await next(context);
    }
}

file static class Log
{
    public static void LogComponentDetected(
        this IConsoleLogger console,
        FileInfo component)
    {
        console.Information(
            $"Component detected:{component.Directory?.Name.ToLink(component)} [dim]{component.FullName}[/]");
    }
}
