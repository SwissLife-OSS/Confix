using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions.Configuration;

public sealed class RepositoryConfiguration
{
    private static class FieldNames
    {
        public const string Component = "component";
        public const string Project = "project";
    }

    public RepositoryConfiguration(ProjectConfiguration? project, ComponentConfiguration? component)
    {
        Project = project;
        Component = component;
    }

    public ComponentConfiguration? Component { get; }

    public ProjectConfiguration? Project { get; }

    public static RepositoryConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var component = obj.TryGetPropertyValue(FieldNames.Component, out var componentNode)
            ? ComponentConfiguration.Parse(componentNode.ExpectObject())
            : null;

        var project = obj.TryGetPropertyValue(FieldNames.Project, out var projectNode)
            ? ProjectConfiguration.Parse(projectNode.ExpectObject())
            : null;

        return new RepositoryConfiguration(project, component);
    }
}
