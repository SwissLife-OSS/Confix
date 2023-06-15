using System.Text.Json.Serialization;
using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Schema;

namespace Confix.Tool.Abstractions;

public sealed class SolutionDefinition
{
    public SolutionDefinition(
        ProjectDefinition? project,
        ComponentDefinition? component,
        DirectoryInfo? directory)
    {
        Project = project;
        Component = component;
        Directory = directory;
    }

    public ComponentDefinition? Component { get; }

    public ProjectDefinition? Project { get; }

    [JsonIgnore]
    public DirectoryInfo? Directory { get; }

    public static SolutionDefinition From(SolutionConfiguration configuration)
    {
        var lastConfigurationFile =
            configuration.SourceFiles.LastOrDefault(x => x.Name == FileNames.ConfixSolution);

        var project = configuration.Project is not null
            ? ProjectDefinition.From(configuration.Project)
            : null;

        var component = configuration.Component is not null
            ? ComponentDefinition.From(configuration.Component)
            : null;

        return new SolutionDefinition(project, component, lastConfigurationFile?.Directory);
    }
}
