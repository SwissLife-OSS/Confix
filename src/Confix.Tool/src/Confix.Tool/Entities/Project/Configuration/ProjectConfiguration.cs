using System.Text.Json.Nodes;
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
    }

    public ProjectConfiguration(
        string? name,
        IReadOnlyList<EnvironmentConfiguration>? environments,
        IReadOnlyList<Component>? components,
        IReadOnlyList<ComponentRepositoryConfiguration>? repositories,
        IReadOnlyList<VariableProviderConfiguration>? variableProviders,
        IReadOnlyList<ComponentProviderConfiguration>? componentProviders,
        IReadOnlyList<ConfigurationFileConfiguration>? configurationFiles,
        IReadOnlyList<ProjectConfiguration>? subprojects)
    {
        Name = name;
        Environments = environments;
        Components = components;
        Repositories = repositories;
        VariableProviders = variableProviders;
        ComponentProviders = componentProviders;
        ConfigurationFiles = configurationFiles;
        Subprojects = subprojects;
    }

    public string? Name { get; }

    public IReadOnlyList<EnvironmentConfiguration>? Environments { get; }

    public IReadOnlyList<Component>? Components { get; }

    public IReadOnlyList<ComponentRepositoryConfiguration>? Repositories { get; }

    public IReadOnlyList<VariableProviderConfiguration>? VariableProviders { get; }

    public IReadOnlyList<ComponentProviderConfiguration>? ComponentProviders { get; }

    public IReadOnlyList<ConfigurationFileConfiguration>? ConfigurationFiles { get; }

    public IReadOnlyList<ProjectConfiguration>? Subprojects { get; }

    public static ProjectConfiguration Parse(JsonNode node)
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
            .Select(property => Component.Parse(property.Key, property.Value!))
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

        return new ProjectConfiguration(
            name,
            environments,
            components,
            repositories,
            variableProviders,
            componentProviders,
            configurationFiles,
            subprojects);
    }
}
