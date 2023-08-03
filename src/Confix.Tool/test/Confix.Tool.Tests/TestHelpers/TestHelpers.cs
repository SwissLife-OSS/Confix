using System.Collections.Immutable;
using System.Text.Json.Nodes;
using Confix.Extensions;
using Confix.Tool;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Middlewares;

namespace Confix.Inputs;

public sealed class TestHelpers
{
    public static ProjectDefinition CreateProjectDefinition(
        string? name = null,
        IReadOnlyList<EnvironmentDefinition>? environments = null,
        IReadOnlyList<ComponentReferenceDefinition>? components = null,
        IReadOnlyList<ComponentRepositoryDefinition>? repositories = null,
        IReadOnlyList<VariableProviderDefinition>? variableProviders = null,
        IReadOnlyList<ComponentProviderDefinition>? componentProviders = null,
        IReadOnlyList<ConfigurationFileDefinition>? configurationFiles = null,
        IReadOnlyList<ProjectDefinition>? subprojects = null,
        ProjectType? projectType = null,
        DirectoryInfo? directory = null)

    {
        return new ProjectDefinition(
            name ?? ProjectDefinition.DefaultName,
            environments ?? Array.Empty<EnvironmentDefinition>(),
            components ?? Array.Empty<ComponentReferenceDefinition>(),
            repositories ?? Array.Empty<ComponentRepositoryDefinition>(),
            variableProviders ?? Array.Empty<VariableProviderDefinition>(),
            componentProviders ?? Array.Empty<ComponentProviderDefinition>(),
            configurationFiles ?? Array.Empty<ConfigurationFileDefinition>(),
            subprojects ?? Array.Empty<ProjectDefinition>(),
            projectType ?? ProjectType.Default,
            directory);
    }

    public static ComponentDefinition CreateComponentDefinition(
        string? name = null,
        IReadOnlyList<ComponentInputDefinition>? inputs = null,
        IReadOnlyList<ComponentOutputDefinition>? outputs = null)
    {
        return new ComponentDefinition(
            name ?? ComponentDefinition.DefaultName,
            inputs ?? Array.Empty<ComponentInputDefinition>(),
            outputs ?? Array.Empty<ComponentOutputDefinition>());
    }

    public static SolutionDefinition CreateSolutionDefinition(
        ProjectDefinition? project = null,
        ComponentDefinition? component = null,
        DirectoryInfo? directory = null)
    {
        return new SolutionDefinition(
            project,
            component,
            directory);
    }

    public static EncryptionDefinition CreateEncryptionDefinition(
        EncryptionProviderDefinition? provider = null)
    {
        return new EncryptionDefinition(
            provider ?? CreateEncryptionProviderDefinition());
    }

    public static EncryptionProviderDefinition CreateEncryptionProviderDefinition(
        string? name = null,
        IReadOnlyDictionary<string, JsonObject>? parameters = null,
        JsonObject? value = null)
    {
        return new EncryptionProviderDefinition(
            name ?? "AES",
            parameters ??
            (IReadOnlyDictionary<string, JsonObject>) ImmutableDictionary<string, JsonObject>.Empty,
            value ?? new JsonObject());
    }

    public static ConfigurationFileCollection CreateConfigurationFileCollection(
        RuntimeConfiguration? configuration = null,
        SolutionConfiguration? solutionConfiguration = null,
        ProjectConfiguration? projectConfiguration = null,
        ComponentConfiguration? componentConfiguration = null,
        IReadOnlyList<JsonFile>? collection = null)
    {
        return new ConfigurationFileCollection(
            configuration,
            solutionConfiguration,
            projectConfiguration,
            componentConfiguration,
            collection ?? Array.Empty<JsonFile>());
    }

    public static ConfigurationFeature CreateConfigurationFeature(
        ConfigurationScope scope,
        ConfigurationFileCollection? configurationFiles = null,
        ProjectDefinition? project = null,
        ComponentDefinition? component = null,
        SolutionDefinition? solution = null,
        EncryptionDefinition? encryption = null)
    {
        return new ConfigurationFeature(
            scope,
            configurationFiles ?? CreateConfigurationFileCollection(),
            project,
            component,
            solution,
            encryption);
    }
}
