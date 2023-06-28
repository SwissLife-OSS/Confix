using System.Text.Json.Serialization;
using Confix.Tool.Abstractions;

namespace Confix.Tool.Middlewares;

public class ConfigurationFeature
{
    public ConfigurationFeature(
        ConfigurationScope scope,
        IConfigurationFileCollection configurationFiles,
        ProjectDefinition? project,
        ComponentDefinition? component,
        SolutionDefinition? solution)
    {
        Scope = scope;
        ConfigurationFiles = configurationFiles;
        Project = project;
        Component = component;
        Solution = solution;
    }

    public ConfigurationScope Scope { get; }

    [JsonIgnore]
    public IConfigurationFileCollection ConfigurationFiles { get; }

    public ProjectDefinition? Project { get; }

    public ComponentDefinition? Component { get; }

    public SolutionDefinition? Solution { get; }
}
