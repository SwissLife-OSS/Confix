using System.Collections;
using ConfiX.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

public sealed class LoadConfigurationMiddleware : IMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.SetStatus("Loading configuration...");

        var configurationFiles = context.LoadConfigurationFiles();

        var fileCollection = CreateFileCollection(configurationFiles);

        var scope = fileCollection.LastOrDefault()?.Name switch
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

        return next(context);
    }

    private static IConfigurationFileCollection CreateFileCollection(
        IEnumerable<FileInfo> configurationFiles)
    {
        var files = new List<FileInfo>(configurationFiles);

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
}

file sealed class ConfigurationFileCollection
    : IConfigurationFileCollection
{
    private readonly IReadOnlyList<FileInfo> _collection;

    public ConfigurationFileCollection(
        RuntimeConfiguration? configuration,
        RepositoryConfiguration? repositoryConfiguration,
        ProjectConfiguration? projectConfiguration,
        ComponentConfiguration? componentConfiguration,
        IReadOnlyList<FileInfo> collection)
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
    public IEnumerator<FileInfo> GetEnumerator()
        => _collection.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc />
    public int Count => _collection.Count;

    /// <inheritdoc />
    public FileInfo this[int index] => _collection[index];
}

file static class Extensions
{
    public static IEnumerable<FileInfo> LoadConfigurationFiles(this IMiddlewareContext context)
    {
        foreach (var confixRc in context.LoadConfixRcs())
        {
            yield return confixRc;
        }

        var repositoryPath = FileSystemHelpers
            .FindInTree(context.Execution.CurrentDirectory, FileNames.ConfixRepository);
        if (repositoryPath is not null)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixRepository, repositoryPath);
            yield return new FileInfo(repositoryPath);
        }

        var confixProject = FileSystemHelpers
            .FindInTree(context.Execution.CurrentDirectory, FileNames.ConfixProject);

        if (confixProject is not null)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixRepository, confixProject);
            yield return new FileInfo(confixProject);
        }

        var confixComponent = FileSystemHelpers
            .FindInTree(context.Execution.CurrentDirectory, FileNames.ConfixComponent);

        if (confixComponent is not null)
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixComponent, confixComponent);
            yield return new FileInfo(confixComponent);
        }
    }

    private static IEnumerable<FileInfo> LoadConfixRcs(this IMiddlewareContext context)
    {
        var confixRcInHome =
            FileSystemHelpers.FindInPath(context.Execution.HomeDirectory,
                FileNames.ConfixRc,
                false);

        if (File.Exists(confixRcInHome))
        {
            App.Log.ConfigurationFilesLocated(FileNames.ConfixRc, confixRcInHome);
            yield return new FileInfo(confixRcInHome);
        }

        var confixRcsInTree = FileSystemHelpers
            .FindAllInTree(context.Execution.CurrentDirectory, FileNames.ConfixRc)
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
