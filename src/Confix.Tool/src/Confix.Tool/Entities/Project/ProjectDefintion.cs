using System.Text.Json.Serialization;
using Confix.Tool.Schema;

namespace Confix.Tool.Abstractions;

public sealed class ProjectDefinition
{
    public const string DefaultName = "__Default";

    public ProjectDefinition(
        string name,
        IReadOnlyList<EnvironmentDefinition> environments,
        IReadOnlyList<ComponentReferenceDefinition> components,
        IReadOnlyList<ComponentRepositoryDefinition> repositories,
        IReadOnlyList<VariableProviderDefinition> variableProviders,
        IReadOnlyList<ComponentProviderDefinition> componentProviders,
        IReadOnlyList<ConfigurationFileDefinition> configurationFiles,
        IReadOnlyList<ProjectDefinition> subprojects,
        ProjectType projectType,
        DirectoryInfo? directory)
    {
        Name = name;
        Environments = environments;
        Components = components;
        Repositories = repositories;
        VariableProviders = variableProviders;
        ComponentProviders = componentProviders;
        ConfigurationFiles = configurationFiles;
        Subprojects = subprojects;
        ProjectType = projectType;
        Directory = directory;
    }

    public string Name { get; }

    public IReadOnlyList<EnvironmentDefinition> Environments { get; }

    public IReadOnlyList<ComponentReferenceDefinition> Components { get; }

    public IReadOnlyList<ComponentRepositoryDefinition> Repositories { get; }

    public IReadOnlyList<VariableProviderDefinition> VariableProviders { get; }

    public IReadOnlyList<ComponentProviderDefinition> ComponentProviders { get; }

    public IReadOnlyList<ConfigurationFileDefinition> ConfigurationFiles { get; }

    public IReadOnlyList<ProjectDefinition> Subprojects { get; }

    public ProjectType ProjectType { get; }

    [JsonIgnore]
    public DirectoryInfo? Directory { get; }

    public static ProjectDefinition From(ProjectConfiguration configuration)
    {
        var lastConfigurationFile =
            configuration.SourceFiles.LastOrDefault(x => x.File.Name == FileNames.ConfixProject);

        var name = configuration.Name
            ?? lastConfigurationFile?.File.Directory?.Name
            ?? DefaultName;

        var environments =
            configuration.Environments?.Select(EnvironmentDefinition.From).ToArray() ??
            Array.Empty<EnvironmentDefinition>();

        var components =
            configuration.Components?.Select(ComponentReferenceDefinition.From).ToArray() ??
            Array.Empty<ComponentReferenceDefinition>();

        var repositories =
            configuration.Repositories?.Select(ComponentRepositoryDefinition.From).ToArray() ??
            Array.Empty<ComponentRepositoryDefinition>();

        var variableProviders =
            configuration.VariableProviders?.Select(VariableProviderDefinition.From).ToArray() ??
            Array.Empty<VariableProviderDefinition>();

        var componentProviders =
            configuration.ComponentProviders?.Select(ComponentProviderDefinition.From).ToArray() ??
            Array.Empty<ComponentProviderDefinition>();

        var configurationFiles =
            configuration.ConfigurationFiles?.Select(ConfigurationFileDefinition.From).ToArray() ??
            Array.Empty<ConfigurationFileDefinition>();

        var subprojects = configuration.Subprojects?.Select(From).ToArray() ??
            Array.Empty<ProjectDefinition>();

        var projectType = configuration.ProjectType switch
        {
            "component" => ProjectType.Component,
            _ => ProjectType.Default
        };

        return new ProjectDefinition(
            name,
            environments,
            components,
            repositories,
            variableProviders,
            componentProviders,
            configurationFiles,
            subprojects,
            projectType,
            lastConfigurationFile?.File.Directory);
    }

    public static ProjectDefinition Instance { get; } = new(
        "root",
        Array.Empty<EnvironmentDefinition>(),
        Array.Empty<ComponentReferenceDefinition>(),
        Array.Empty<ComponentRepositoryDefinition>(),
        Array.Empty<VariableProviderDefinition>(),
        Array.Empty<ComponentProviderDefinition>(),
        Array.Empty<ConfigurationFileDefinition>(),
        Array.Empty<ProjectDefinition>(),
        ProjectType.Default,
        null);
}
