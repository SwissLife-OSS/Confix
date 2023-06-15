using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;

namespace Confix.Tool.Commands.Solution;

public sealed class ComponentInitPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder.UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Initialize the component...");

        var executingDirectory = context.Execution.CurrentDirectory;
        var componentFilePath =
            Path.Combine(executingDirectory.FullName, FileNames.ConfixComponent);
        var componentFile = new FileInfo(componentFilePath);

        if (componentFile.Exists)
        {
            context.Logger.LogComponentAlreadyExists(componentFile);
            throw new ExitException();
        }

        await File.WriteAllTextAsync(componentFile.FullName, "{}");
        context.Logger.LogComponentCreated(componentFile);
    }
}

file static class Log
{
    public static void LogComponentAlreadyExists(
        this IConsoleLogger console,
        FileInfo componentFile)
    {
        console.Error($"Component already exists: [grey]{componentFile.FullName}[/]");
    }

    public static void LogComponentCreated(
        this IConsoleLogger console,
        FileInfo componentFile)
    {
        console.Information($"Component created: [grey]{componentFile.FullName}[/]");
    }
}
