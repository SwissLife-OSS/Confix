using System.Text.Json;
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

    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        if (Project is not null)
        {
            writer.WritePropertyName(SolutionConfiguration.FieldNames.Project);
            Project.WriteTo(writer);
        }

        if (Component is not null)
        {
            writer.WritePropertyName(SolutionConfiguration.FieldNames.Component);
            Component.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    public static SolutionDefinition From(SolutionConfiguration configuration)
    {
        var lastConfigurationFile =
            configuration.SourceFiles.LastOrDefault(x => x.File.Name == FileNames.ConfixSolution);

        var project = configuration.Project is not null
            ? ProjectDefinition.From(configuration.Project)
            : null;

        var component = configuration.Component is not null
            ? ComponentDefinition.From(configuration.Component)
            : null;

        return new SolutionDefinition(project, component, lastConfigurationFile?.File.Directory);
    }
}
