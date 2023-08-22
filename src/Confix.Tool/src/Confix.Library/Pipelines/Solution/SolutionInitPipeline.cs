using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;

namespace Confix.Tool.Commands.Solution;

public sealed class SolutionInitPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder.UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Initialize the solution...");

        var executingDirectory = context.Execution.CurrentDirectory;
        var solutionFilePath = Path.Combine(executingDirectory.FullName, FileNames.ConfixSolution);
        var solutionFile = new FileInfo(solutionFilePath);

        if (solutionFile.Exists)
        {
            var info = solutionFile.Directory?.Name.ToLink(solutionFile);
            throw new ExitException(
                $"Solution already exists: {info} [dim]{solutionFile.FullName}[/]");
        }

        await File.WriteAllTextAsync(solutionFile.FullName, "{}");
        context.Logger.LogSolutionCreated(solutionFile);
    }
}

file static class Log
{
    public static void LogSolutionCreated(
        this IConsoleLogger console,
        FileInfo info)
    {
        console.Information(
            $"Solution created: {info.Directory?.Name.ToLink(info)} [dim]{info.FullName}[/]");
    }
}
