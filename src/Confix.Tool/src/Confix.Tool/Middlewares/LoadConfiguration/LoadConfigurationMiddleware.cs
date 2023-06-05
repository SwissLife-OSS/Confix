using System.Collections;
using System.CommandLine;
using ConfiX.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

public sealed class LoadConfigurationMiddleware : IMiddleware
{

    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configurationFiles = context.LoadConfigurationFiles();

        var fileCollection = CreateFileCollection(configurationFiles);

        var scope = fileCollection.LastOrDefault()?.Name switch
        {
            FileNames.ConfixComponent => ConfigurationScope.Component,
            FileNames.ConfixProject => ConfigurationScope.Project,
            FileNames.ConfixRepository => ConfigurationScope.Repository,
            _ => ConfigurationScope.None
        };

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
            projectConfiguration = project?.Merge(projectConfiguration);
        }

        var componentConfiguration = ComponentConfiguration.LoadFromFiles(files);
        var component = repositoryConfiguration?.Component ?? confixConfiguration.Component;
        if (component is not null)
        {
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
            yield return new FileInfo(repositoryPath);
        }

        var confixProject = FileSystemHelpers
            .FindInTree(context.Execution.CurrentDirectory, FileNames.ConfixProject);

        if (confixProject is not null)
        {
            yield return new FileInfo(confixProject);
        }

        var confixComponent = FileSystemHelpers
            .FindInTree(context.Execution.CurrentDirectory, FileNames.ConfixComponent);

        if (confixComponent is not null)
        {
            yield return new FileInfo(confixComponent);
        }
    }

    private static IEnumerable<FileInfo> LoadConfixRcs(this IMiddlewareContext context)
    {
        var confixRcInHome =
            FileSystemHelpers.FindInPath(context.Execution.HomeDirectory, FileNames.ConfixRc, false);

        if (File.Exists(confixRcInHome))
        {
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

            yield return new FileInfo(confixRcInTree);
        }
    }
}
