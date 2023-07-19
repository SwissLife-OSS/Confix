using System.Text.Json.Nodes;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class ProjectConfiguration
{
    private static class FieldNames
    {
        public const string Name = "name";
        public const string Environments = "environments";
        public const string Components = "components";
        public const string ComponentRepositories = "componentRepositories";
        public const string VariableProviders = "variableProviders";
        public const string ComponentProviders = "componentProviders";
        public const string ConfigurationFiles = "configurationFiles";
        public const string Subprojects = "subprojects";
        public const string ProjectType = "type";
    }

    public ProjectConfiguration(
        string? name,
        IReadOnlyList<EnvironmentConfiguration>? environments,
        IReadOnlyList<ComponentReferenceConfiguration>? components,
        IReadOnlyList<ComponentRepositoryConfiguration>? repositories,
        IReadOnlyList<VariableProviderConfiguration>? variableProviders,
        IReadOnlyList<ComponentProviderConfiguration>? componentProviders,
        IReadOnlyList<ConfigurationFileConfiguration>? configurationFiles,
        IReadOnlyList<ProjectConfiguration>? subprojects,
        string? projectType,
        IReadOnlyList<JsonFile> sourceFiles)
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
        SourceFiles = sourceFiles;
    }

    public string? Name { get; }

    public IReadOnlyList<EnvironmentConfiguration>? Environments { get; }

    public IReadOnlyList<ComponentReferenceConfiguration>? Components { get; }

    public IReadOnlyList<ComponentRepositoryConfiguration>? Repositories { get; }

    public IReadOnlyList<VariableProviderConfiguration>? VariableProviders { get; }

    public IReadOnlyList<ComponentProviderConfiguration>? ComponentProviders { get; }

    public IReadOnlyList<ConfigurationFileConfiguration>? ConfigurationFiles { get; }

    public IReadOnlyList<ProjectConfiguration>? Subprojects { get; }

    public IReadOnlyList<JsonFile> SourceFiles { get; }

    public string? ProjectType { get; }

    public static ProjectConfiguration Parse(JsonNode? node)
        => Parse(node, Array.Empty<JsonFile>());

    public static ProjectConfiguration Parse(JsonNode? node, IReadOnlyList<JsonFile> sourceFiles)
    {
        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var environments = obj
            .MaybeProperty(FieldNames.Environments)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(EnvironmentConfiguration.Parse)
            .ToArray();

        var components = obj
            .MaybeProperty(FieldNames.Components)
            ?.ExpectObject()
            .Where(x => x.Value is not null)
            .Select(property
                => ComponentReferenceConfiguration.Parse(property.Key, property.Value!))
            .ToArray();

        var repositories = obj
            .MaybeProperty(FieldNames.ComponentRepositories)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(ComponentRepositoryConfiguration.Parse)
            .ToArray();

        var variableProviders = obj
            .MaybeProperty(FieldNames.VariableProviders)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(VariableProviderConfiguration.Parse)
            .ToArray();

        var componentProviders = obj
            .MaybeProperty(FieldNames.ComponentProviders)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(ComponentProviderConfiguration.Parse)
            .ToArray();

        var configurationFiles = obj
            .MaybeProperty(FieldNames.ConfigurationFiles)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(ConfigurationFileConfiguration.Parse)
            .ToArray();

        var subprojects = obj
            .MaybeProperty(FieldNames.Subprojects)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(Parse)
            .ToArray();

        var projectType = obj
            .MaybeProperty(FieldNames.ProjectType)
            ?.ExpectValue<string>();

        return new ProjectConfiguration(
            name,
            environments,
            components,
            repositories,
            variableProviders,
            componentProviders,
            configurationFiles,
            subprojects,
            projectType,
            sourceFiles);
    }

    public ProjectConfiguration Merge(ProjectConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var name = other.Name ?? Name;

        var environments = (Environments, other.Environments)
            .MergeWith((x, y) => x.Name == y.Name, (x, y) => x.Merge(y));

        var components = (Components, other.Components)
            .MergeWith(
                (x, y) => x.Provider == y.Provider && x.ComponentName == y.ComponentName,
                (x, y) => x.Merge(y));

        var repositories = (Repositories, other.Repositories)
            .MergeWith((x, y) => x.Name == y.Name, (x, y) => x.Merge(y));

        var variableProviders = (VariableProviders, other.VariableProviders)
            .MergeWith((x, y) => x.Name == y.Name, (x, y) => x.Merge(y));

        var componentProviders = (ComponentProviders, other.ComponentProviders)
            .MergeWith((x, y) => x.Name == y.Name, (x, y) => x.Merge(y));

        var configurationFiles = other.ConfigurationFiles ?? ConfigurationFiles;

        var subprojects = (Subprojects, other.Subprojects)
            .MergeWith((x, y) => x.Name == y.Name, (x, y) => x.Merge(y));

        var sourceFiles = SourceFiles.Concat(other.SourceFiles).ToArray();

        var projectType = other.ProjectType ?? ProjectType;

        return new ProjectConfiguration(
            name,
            environments,
            components,
            repositories,
            variableProviders,
            componentProviders,
            configurationFiles,
            subprojects,
            projectType,
            sourceFiles);
    }

    public static ProjectConfiguration? LoadFromFiles(IEnumerable<JsonFile> files)
    {
        var config = files.FirstOrDefault(x => x.File.Name == FileNames.ConfixProject);
        if (config is null)
        {
            return null;
        }

        var json = config.Content;
        return Parse(json, new[] { config });
    }

    public static ProjectConfiguration Empty { get; } =
        new(null, null, null, null, null, null, null, null, null, Array.Empty<JsonFile>());
}
