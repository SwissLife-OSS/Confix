using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Confix.Utilities.FileSystem;

namespace Confix.Tool.Entities.Components;

public sealed class DotnetComponentInput : IComponentInput
{
    private const string _embeddedResourcePath =
        $"$(MSBuildProjectDirectory)/{FolderNames.Confix}/{FolderNames.Components}/**/*.*";

    public static string Type => "dotnet";

    /// <inheritdoc />
    public async Task ExecuteAsync(IMiddlewareContext context)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        if (configuration.Scope is not ConfigurationScope.Component ||
            configuration.ConfigurationFiles.Component is null)
        {
            throw new ExitException("Component input has to be executed in a component directory");
        }

        if (configuration.Project?.Directory is not { } projectDirectory)
        {
            context.Logger.DotnetComponentProviderNeedsToBeRunInsideAConfixProject();
            return;
        }

        var csproj = DotnetHelpers.FindProjectFileInPath(projectDirectory);

        if (csproj is null)
        {
            context.Logger.ProjectNotFoundInDirectory(projectDirectory);
            context.Logger.DotnetProjectWasNotDetected();
            return;
        }

        context.Logger.FoundDotnetProject(csproj);

        try
        {
            await DotnetHelpers.EnsureEmbeddedResourceAsync(
                csproj, 
                _embeddedResourcePath, 
                context.CancellationToken);
        }
        catch
        {
            context.Logger.CouldNotParseProjectFile(csproj);
        }
    }
}

file static class Log
{
    public static void DotnetComponentProviderNeedsToBeRunInsideAConfixProject(
        this IConsoleLogger console)
    {
        console.Error(
            "Dotnet component provider needs to be run inside a confix project. No project was found");
    }

    public static void ProjectNotFoundInDirectory(
        this IConsoleLogger logger,
        DirectoryInfo directory)
    {
        logger.Debug($"Could not find project in directory: {directory}");
    }

    public static void DotnetProjectWasNotDetected(this IConsoleLogger logger)
    {
        logger.Information("Dotnet project was not detected. Skipping dotnet component input");
    }

    public static void FoundDotnetProject(this IConsoleLogger logger, FileSystemInfo csproj)
    {
        logger.Information($"Found .NET project:{csproj.ToLink()} [dim]{csproj.FullName}[/]");
    }

    public static void CouldNotParseProjectFile(this IConsoleLogger logger, FileSystemInfo csproj)
    {
        logger.Error($"Could not parse project file: {csproj.ToLink()} [dim]{csproj.FullName}[/]");
    }
}
