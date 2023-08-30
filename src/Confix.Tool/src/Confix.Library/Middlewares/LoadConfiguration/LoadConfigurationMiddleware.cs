using System.Collections;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;
using System.Runtime.CompilerServices;
using Confix.Extensions;

namespace Confix.Tool.Middlewares;

public sealed class LoadConfigurationMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        if (context.Features.TryGet(out ConfigurationFeature _))
        {
            await next(context);
            return;
        }

        context.SetStatus("Loading configuration...");

        var configurationFeature = await LoadConfiguration(context);
        
        context.Features.Set(configurationFeature);

        context.Logger.ConfigurationLoaded();

        await next(context);
    }

    private static async Task<ConfigurationFeature> LoadConfiguration(IMiddlewareContext context)
    {
        var configurationFiles = await context
            .LoadConfigurationFiles(context.CancellationToken)
            .ToListAsync(context.CancellationToken);

        var configurationFilesWithReplacedMagicStrings =
            ReplaceMagicStrings(context, configurationFiles);
        var fileCollection = CreateFileCollection(configurationFilesWithReplacedMagicStrings);

        var scope = fileCollection.LastOrDefault()?.File.Name switch
        {
            FileNames.ConfixComponent => ConfigurationScope.Component,
            FileNames.ConfixProject => ConfigurationScope.Project,
            FileNames.ConfixSolution => ConfigurationScope.Solution,
            _ => ConfigurationScope.None
        };

        context.Logger.RunningInScope(scope, fileCollection);

        var projectDefinition = fileCollection.Project is not null
            ? ProjectDefinition.From(fileCollection.Project)
            : null;

        var componentDefinition = fileCollection.Component is not null
            ? ComponentDefinition.From(fileCollection.Component)
            : null;

        var solutionDefinition = fileCollection.Solution is not null
            ? SolutionDefinition.From(fileCollection.Solution)
            : null;

        var encryptionDefinition = fileCollection.RuntimeConfiguration?.Encryption is not null
            ? EncryptionDefinition.From(fileCollection.RuntimeConfiguration.Encryption)
            : null;

        return new ConfigurationFeature(
            scope,
            fileCollection,
            projectDefinition,
            componentDefinition,
            solutionDefinition,
            encryptionDefinition);
    }

    private static IConfigurationFileCollection CreateFileCollection(
        IEnumerable<JsonFile> configurationFiles)
    {
        var files = new List<JsonFile>(configurationFiles);

        var confixConfiguration = RuntimeConfiguration.LoadFromFiles(files);
        var solutionConfiguration = SolutionConfiguration.LoadFromFiles(files);

        if (solutionConfiguration is not null &&
            confixConfiguration is { Project: not null } or { Component: not null })
        {
            App.Log.MergedConfigurationFiles(FileNames.ConfixSolution, FileNames.ConfixRc);

            solutionConfiguration = new SolutionConfiguration(
                    confixConfiguration.Project,
                    confixConfiguration.Component,
                    confixConfiguration.SourceFiles)
                .Merge(solutionConfiguration);
        }

        var projectConfiguration = ProjectConfiguration.LoadFromFiles(files);
        var project = solutionConfiguration?.Project ?? confixConfiguration.Project;
        if (project is not null)
        {
            App.Log.MergedConfigurationFiles(FileNames.ConfixProject, FileNames.ConfixSolution);
            projectConfiguration = project?.Merge(projectConfiguration);
        }

        var componentConfiguration = ComponentConfiguration.LoadFromFiles(files);
        var component = solutionConfiguration?.Component ?? confixConfiguration.Component;
        if (component is not null)
        {
            App.Log.MergedConfigurationFiles(FileNames.ConfixComponent, FileNames.ConfixSolution);
            componentConfiguration = component.Merge(componentConfiguration);
        }

        return new ConfigurationFileCollection(
            confixConfiguration,
            solutionConfiguration,
            projectConfiguration,
            componentConfiguration,
            files);
    }

    private static IEnumerable<JsonFile> ReplaceMagicStrings(
        IMiddlewareContext middlewareContext,
        IEnumerable<JsonFile> configurationFiles)
    {
        var solutionFile = configurationFiles
            .FirstOrDefault(x => x.File.Name == FileNames.ConfixSolution);
        var projectFile = configurationFiles
            .FirstOrDefault(x => x.File.Name == FileNames.ConfixProject);

        foreach (var file in configurationFiles)
        {
            MagicPathContext context = new(
                middlewareContext.Execution.CurrentDirectory,
                solutionFile?.File.Directory,
                projectFile?.File.Directory,
                file.File.Directory!);

            var rewritten = new MagicPathRewriter().Rewrite(file.Content, context);

            yield return file with { Content = rewritten };
        }
    }
}

file static class Extensions
{
    public static async IAsyncEnumerable<JsonFile> LoadConfigurationFiles(
        this IMiddlewareContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var confixRc in context.LoadConfixRcs())
        {
            yield return await JsonFile.FromFile(confixRc, cancellationToken);
        }

        var solutionPath =
            context.Execution.CurrentDirectory.FindInTree(FileNames.ConfixSolution);

        if (solutionPath is not null)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixSolution, solutionPath);
            yield return await JsonFile.FromFile(new(solutionPath), cancellationToken);
        }

        var confixProject = context.Execution.CurrentDirectory.FindInTree(FileNames.ConfixProject);

        if (confixProject is not null)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixSolution, confixProject);
            yield return await JsonFile.FromFile(new(confixProject), cancellationToken);
        }

        var confixComponent =
            context.Execution.CurrentDirectory.FindInTree(FileNames.ConfixComponent);

        if (confixComponent is not null)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixComponent, confixComponent);
            yield return await JsonFile.FromFile(new(confixComponent), cancellationToken);
        }
    }

    private static IEnumerable<FileInfo> LoadConfixRcs(this IMiddlewareContext context)
    {
        var confixRcInHome = context.Execution.HomeDirectory.FindInPath(FileNames.ConfixRc, false);

        if (confixRcInHome?.Exists is true)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixRc, confixRcInHome.FullName);
            yield return confixRcInHome;
        }

        var confixRcsInTree = context.Execution.CurrentDirectory
            .FindAllInTree(FileNames.ConfixRc)
            .Reverse();

        foreach (var confixRcInTree in confixRcsInTree)
        {
            if (confixRcInTree == confixRcInHome)
            {
                continue;
            }

            App.Log.ConfigurationFilesLocated(FileNames.ConfixRc, confixRcInTree.FullName);
            yield return confixRcInTree;
        }
    }
}

file static class Log
{
    public static void ConfigurationFilesLocated(
        this IConsoleLogger logger,
        string type,
        string path) => logger.Debug("Configuration files of type {0} located at {1}", type, path);

    public static void MergedConfigurationFiles(
        this IConsoleLogger logger,
        string source,
        string destination)
        => logger.Debug("Merged {0} from {1}", source, destination);

    public static void RunningInScope(
        this IConsoleLogger logger,
        ConfigurationScope scope,
        IConfigurationFileCollection fileCollection)
    {
        if (scope is ConfigurationScope.None)
        {
            logger.Warning(
                "Could not determine configuration scope. This means that no .confix.project, .confix.component or .confix.solution file was found.");
        }
        else
        {
            logger.Success("Running in scope {0}", scope.ToString().AsHighlighted());
        }
    }

    public static void ConfigurationLoaded(this IConsoleLogger logger)
        => logger.Success("Configuration loaded");
}
