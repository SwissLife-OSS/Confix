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

        var configurationFiles =  context.LoadConfigurationFiles(context.CancellationToken).ToBlockingEnumerable();
        var configurationFilesWithReplacedMagicStrings = ReplaceMagicStrings(context, configurationFiles);
        var fileCollection = CreateFileCollection(configurationFilesWithReplacedMagicStrings);

        var scope = fileCollection.LastOrDefault()?.File.Name switch
        {
            FileNames.ConfixComponent => ConfigurationScope.Component,
            FileNames.ConfixProject => ConfigurationScope.Project,
            FileNames.ConfixRepository => ConfigurationScope.Repository,
            _ => ConfigurationScope.None
        };

        context.Logger.RunningInScope(scope);

        var projectDefinition = fileCollection.Project is not null
            ? ProjectDefinition.From(fileCollection.Project)
            : null;

        var componentDefinition = fileCollection.Component is not null
            ? ComponentDefinition.From(fileCollection.Component)
            : null;

        var repositoryDefinition = fileCollection.Repository is not null
            ? RepositoryDefinition.From(fileCollection.Repository)
            : null;

        var feature = new ConfigurationFeature(
            scope,
            fileCollection,
            projectDefinition,
            componentDefinition,
            repositoryDefinition);

        context.Features.Set(feature);

        context.Logger.ConfigurationLoaded();

        await next(context);
    }

    private static IConfigurationFileCollection CreateFileCollection(IEnumerable<JsonFile> configurationFiles)
    {
        var files = new List<JsonFile>(configurationFiles);

        var confixConfiguration = RuntimeConfiguration.LoadFromFiles(files);
        var repositoryConfiguration = RepositoryConfiguration.LoadFromFiles(files);

        if (repositoryConfiguration is not null &&
            confixConfiguration is { Project: not null } or { Component: not null })
        {
            App.Log.MergedConfigurationFiles(FileNames.ConfixRepository, FileNames.ConfixRc);

            repositoryConfiguration = new RepositoryConfiguration(
                    confixConfiguration.Project,
                    confixConfiguration.Component,
                    confixConfiguration.SourceFiles)
                .Merge(repositoryConfiguration);
        }

        var projectConfiguration = ProjectConfiguration.LoadFromFiles(files);
        var project = repositoryConfiguration?.Project ?? confixConfiguration.Project;
        if (project is not null)
        {
            App.Log.MergedConfigurationFiles(FileNames.ConfixProject, FileNames.ConfixRepository);
            projectConfiguration = project?.Merge(projectConfiguration);
        }

        var componentConfiguration = ComponentConfiguration.LoadFromFiles(files);
        var component = repositoryConfiguration?.Component ?? confixConfiguration.Component;
        if (component is not null)
        {
            App.Log.MergedConfigurationFiles(FileNames.ConfixComponent, FileNames.ConfixRepository);
            componentConfiguration = component.Merge(componentConfiguration);
        }

        return new ConfigurationFileCollection(
            confixConfiguration,
            repositoryConfiguration,
            projectConfiguration,
            componentConfiguration,
            files);
    }

    private static IEnumerable<JsonFile> ReplaceMagicStrings(
        IMiddlewareContext middlewareContext,
        IEnumerable<JsonFile> configurationFiles)
    {
        var repositoryFile = configurationFiles.FirstOrDefault(x => x.File.Name == FileNames.ConfixRepository);
        var projectFile = configurationFiles.FirstOrDefault(x => x.File.Name == FileNames.ConfixProject);

        foreach (var file in configurationFiles)
        {
            MagicPathContext context = new(
                    middlewareContext.Execution.CurrentDirectory,
                    repositoryFile?.File.Directory,
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
        RepositoryConfiguration? repositoryConfiguration,
        ProjectConfiguration? projectConfiguration,
        ComponentConfiguration? componentConfiguration,
        IReadOnlyList<JsonFile> collection)
    {
        RuntimeConfiguration = configuration;
        Repository = repositoryConfiguration;
        Project = projectConfiguration;
        Component = componentConfiguration;
        _collection = collection;
    }

    public RuntimeConfiguration? RuntimeConfiguration { get; }

    public RepositoryConfiguration? Repository { get; }

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
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        foreach (var confixRc in context.LoadConfixRcs())
        {
            yield return await JsonFile.FromFile(confixRc, cancellationToken);
        }

        var repositoryPath =
            context.Execution.CurrentDirectory.FindInTree(FileNames.ConfixRepository);

        if (repositoryPath is not null)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixRepository, repositoryPath);
            yield return await JsonFile.FromFile(new(repositoryPath), cancellationToken);
        }

        var confixProject = context.Execution.CurrentDirectory.FindInTree(FileNames.ConfixProject);

        if (confixProject is not null)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixRepository, confixProject);
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

        if (File.Exists(confixRcInHome))
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixRc, confixRcInHome);
            yield return new FileInfo(confixRcInHome);
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

            App.Log.ConfigurationFilesLocated(FileNames.ConfixRc, confixRcInTree);
            yield return new FileInfo(confixRcInTree);
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
                "Could not determine configuration scope. This means that no .confix.project, .confix.component or .confix.repository file was found.");
        }
        else
        {
            logger.Success("Running in scope {0}", scope.ToString().AsHighlighted());
        }
    }

    public static void ConfigurationLoaded(this IConsoleLogger logger)
        => logger.Success("Configuration loaded");
}

