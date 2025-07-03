using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;

namespace Confix.Tool.Commands.Project;

public sealed class ListProjectsPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddOption(DirectoriesOnlyOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static Task InvokeAsync(IMiddlewareContext context)
    {
        var searchDirectory = context.Execution.CurrentDirectory;
        var projectFiles = searchDirectory.FindAllInPath(FileNames.ConfixProject);
        var directoriesOnly = context.Parameter.TryGet(DirectoriesOnlyOption.Instance, out bool dirs) && dirs;

        foreach (var projectFile in projectFiles)
        {
            if (directoriesOnly)
            {
                context.Logger.Information(projectFile.Directory!.FullName);
            }
            else
            {
                context.Logger.Information(projectFile.FullName);
            }
        }

        return Task.CompletedTask;
    }
}
