using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;

namespace Confix.Tool.Commands.Solution;

public sealed class ProjectInitPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder.UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Initialize the project...");

        var executingDirectory = context.Execution.CurrentDirectory;
        var projectFilePath = Path.Combine(executingDirectory.FullName, FileNames.ConfixProject);
        var projectFile = new FileInfo(projectFilePath);

        if (projectFile.Exists)
        {
            context.Logger.LogProjectAlreadyExists(projectFile);
            throw new ExitException();
        }

        await File.WriteAllTextAsync(projectFile.FullName, "{}");
        context.Logger.LogProjectCreated(projectFile);
    }
}

file static class Log
{
    public static void LogProjectAlreadyExists(
        this IConsoleLogger console,
        FileInfo info)
    {
        console.Error(
            $"Project already exists: {info.Directory?.Name.ToLink(info)} [dim]{info.FullName}[/]");
    }

    public static void LogProjectCreated(
        this IConsoleLogger console,
        FileInfo info)
    {
        console.Information(
            $"Project created: {info.Directory?.Name.ToLink(info)} [dim]{info.FullName}[/]");
    }
}
