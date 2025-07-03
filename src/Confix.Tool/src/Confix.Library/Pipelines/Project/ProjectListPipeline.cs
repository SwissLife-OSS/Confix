using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectListPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .AddOption(DirectoriesOnlyOption.Instance)
            .UseHandler(InvokeAsync);
    }

    private static Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Finding all project files...");

        var configuration = context.Features.Get<ConfigurationFeature>();
        var searchDirectory = configuration.Solution?.Directory ?? context.Execution.CurrentDirectory;
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
