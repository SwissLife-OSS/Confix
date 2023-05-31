using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Utilities.Json;

namespace ConfiX.Extensions;

public class ConfixConfiguration
{
    private static class FieldNames
    {
        public const string IsRoot = "isRoot";
        public const string Component = "component";
        public const string Project = "project";
    }

    public ConfixConfiguration(
        bool isRoot,
        ProjectConfiguration? project,
        ComponentConfiguration? component)
    {
        IsRoot = isRoot;
        Project = project;
        Component = component;
    }

    public bool IsRoot { get; set; }

    public ComponentConfiguration? Component { get; }

    public ProjectConfiguration? Project { get; }

    public static ConfixConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var isRoot = obj.TryGetPropertyValue(FieldNames.IsRoot, out var isRootNode) &&
            isRootNode.ExpectValue<bool>();

        var component = obj.TryGetPropertyValue(FieldNames.Component, out var componentNode)
            ? ComponentConfiguration.Parse(componentNode.ExpectObject())
            : null;

        var project = obj.TryGetPropertyValue(FieldNames.Project, out var projectNode)
            ? ProjectConfiguration.Parse(projectNode.ExpectObject())
            : null;

        return new ConfixConfiguration(isRoot, project, component);
    }
}
