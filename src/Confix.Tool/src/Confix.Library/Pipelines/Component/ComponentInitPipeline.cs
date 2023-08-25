using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Confix.Utilities.FileSystem;

namespace Confix.Tool.Commands.Solution;

public sealed class ComponentInitPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddArgument(ComponentNameArgument.Instance)
            .Use<LoadConfigurationMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Initialize the component...");
        var configuration = context.Features.Get<ConfigurationFeature>();

        configuration.EnsureProjectScope();
        var project = configuration.EnsureProject();

        var componentName = context.Parameter.Get(ComponentNameArgument.Instance);
        var componentFolder = project.Directory!
            .Append(FolderNames.Confix)
            .Append(FolderNames.Components)
            .Append(componentName);
        
        componentFolder.EnsureFolder();

        var componentFile = componentFolder.AppendFile(FileNames.ConfixComponent);

        if (componentFile.Exists)
        {
            var link = componentFile.Directory?.Name.ToLink(componentFile);
            throw new ExitException(
                $"Component already exists:{link} [dim]{componentFile.FullName}[/]");
        }

        await File
            .WriteAllTextAsync(componentFile.FullName, $$""" { "name":"{{componentName}}" } """);
        context.Logger.LogComponentCreated(componentFile);
    }
}

file static class Log
{
    public static void LogComponentCreated(
        this IConsoleLogger console,
        FileInfo info)
    {
        console.Information(
            $"Component created:{info.Directory?.Name.ToLink(info)} [dim]{info.FullName}[/]");
    }
}
