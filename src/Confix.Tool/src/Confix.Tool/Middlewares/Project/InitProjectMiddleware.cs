using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares.Project;

public sealed class InitProjectMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        switch (configuration.Scope)
        {
            case ConfigurationScope.Component:
                AssertComponentNotSupported(configuration);
                break;

            case ConfigurationScope.Project:
                await ContinuePipeline(context, next, configuration);
                break;

            case ConfigurationScope.None or ConfigurationScope.Solution:
                await InitializeProjectAndReload(context);
                break;
        }
    }

    private static async Task InitializeProjectAndReload(IMiddlewareContext context)
    {
        context.SetStatus("Initialize the project...");

        var projectFile = await WriteProjectFile(context);

        context.Logger.LogProjectCreated(projectFile);

        // restart the pipeline after the project is created. This way we guarantee that the
        // context is fresh. We could also just reload the configuration, but this is more
        // future proof.
        var pipeline = new ProjectInitPipeline();
        var projectContext = context
            .WithFeatureCollection()
            .WithContextData();

        await pipeline.ExecuteAsync(projectContext);
    }

    private static async Task ContinuePipeline(
        IMiddlewareContext context,
        MiddlewareDelegate next,
        ConfigurationFeature configuration)
    {
        var project = configuration.EnsureProject();
        context.Logger.LogProjectFound(project.Directory!);

        await next(context);
    }

    private static void AssertComponentNotSupported(ConfigurationFeature configuration)
    {
        var componentLocation = configuration.ConfigurationFiles
            .FirstOrDefault(x => x.File.Name == FileNames.ConfixComponent)
            ?.File.Directory?.FullName ?? "unknown";

        throw new ExitException("Cannot initialize a project inside a component.")
        {
            Help =
                $"You are trying to initialize a project inside a component. The component is located {componentLocation}"
        };
    }

    private static async Task<FileInfo> WriteProjectFile(IMiddlewareContext context)
    {
        var executingDirectory = context.Execution.CurrentDirectory;
        var projectFilePath =
            Path.Combine(executingDirectory.FullName, FileNames.ConfixProject);
        var projectFile = new FileInfo(projectFilePath);

        await File.WriteAllTextAsync(projectFile.FullName, "{}");
        return projectFile;
    }
}

file static class Log
{
    public static void LogProjectFound(this IConsoleLogger console, FileSystemInfo info)
    {
        console.Information($"Project found at [dim]{info.FullName}[/]");
    }

    public static void LogProjectCreated(
        this IConsoleLogger console,
        FileInfo info)
    {
        console.Information(
            $"Project created: {info.Directory?.Name.ToLink(info)} [dim]{info.FullName}[/]");
    }
}