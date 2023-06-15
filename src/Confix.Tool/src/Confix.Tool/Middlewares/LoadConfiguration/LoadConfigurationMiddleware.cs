using System.Collections;
using ConfiX.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;
using System.Runtime.CompilerServices;

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

        var configurationFiles = context.LoadConfigurationFiles(context.CancellationToken).ToBlockingEnumerable();
        var configurationFilesWithReplacedMagicStrings = ReplaceMagicStrings(context, configurationFiles);
        var fileCollection = CreateFileCollection(configurationFilesWithReplacedMagicStrings);

        var scope = fileCollection.LastOrDefault()?.File.Name switch
        {
            FileNames.ConfixComponent => ConfigurationScope.Component,
            FileNames.ConfixProject => ConfigurationScope.Project,
            FileNames.ConfixSolution => ConfigurationScope.Solution,
            _ => ConfigurationScope.None
        };

        context.Logger.RunningInScope(scope);

        var projectDefinition = fileCollection.Project is not null
            ? ProjectDefinition.From(fileCollection.Project)
            : null;

        var componentDefinition = fileCollection.Component is not null
            ? ComponentDefinition.From(fileCollection.Component)
            : null;

        var solutionDefinition = fileCollection.Solution is not null
            ? SolutionDefinition.From(fileCollection.Solution)
            : null;

        var feature = new ConfigurationFeature(
            scope,
            fileCollection,
            projectDefinition,
            componentDefinition,
            solutionDefinition);

        context.Features.Set(feature);

        context.Logger.ConfigurationLoaded();

        await next(context);
    }

    private static IConfigurationFileCollection CreateFileCollection(IEnumerable<JsonFile> configurationFiles)
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
        var solutionFile = configurationFiles.FirstOrDefault(x => x.File.Name == FileNames.ConfixSolution);
        var projectFile = configurationFiles.FirstOrDefault(x => x.File.Name == FileNames.ConfixProject);

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

file sealed class ConfigurationFileCollection
    : IConfigurationFileCollection
{
    private readonly IReadOnlyList<JsonFile> _collection;

    public ConfigurationFileCollection(
        RuntimeConfiguration? configuration,
        SolutionConfiguration? solutionConfiguration,
        ProjectConfiguration? projectConfiguration,
        ComponentConfiguration? componentConfiguration,
        IReadOnlyList<JsonFile> collection)
    {
        RuntimeConfiguration = configuration;
        Solution = solutionConfiguration;
        Project = projectConfiguration;
        Component = componentConfiguration;
        _collection = collection;
    }

    public RuntimeConfiguration? RuntimeConfiguration { get; }

    public SolutionConfiguration? Solution { get; }

    public ProjectConfiguration? Project { get; }

    public ComponentConfiguration? Component { get; }

    /// <inheritdoc />
    public IEnumerator<JsonFile> GetEnumerator()
        => _collection.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc />
    public int Count => _collection.Count;

    /// <inheritdoc />
    public JsonFile this[int index] => _collection[index];
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

    public static void RunningInScope(this IConsoleLogger logger, ConfigurationScope scope)
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

