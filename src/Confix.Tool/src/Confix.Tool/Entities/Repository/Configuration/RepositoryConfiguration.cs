using System.Text.Json.Nodes;
using Confix.Tool.Schema;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions.Configuration;

public sealed class RepositoryConfiguration
{
    private static class FieldNames
    {
        public const string Component = "component";
        public const string Project = "project";
    }

    public RepositoryConfiguration(
        ProjectConfiguration? project,
        ComponentConfiguration? component,
        IReadOnlyList<FileInfo> sourceFiles)
    {
        Project = project;
        Component = component;
        SourceFiles = sourceFiles;
    }

    public ComponentConfiguration? Component { get; }

    public ProjectConfiguration? Project { get; }

    public IReadOnlyList<FileInfo> SourceFiles { get; }

    public static RepositoryConfiguration Parse(JsonNode? node)
    {
        return Parse(node, Array.Empty<FileInfo>());
    }

    public static RepositoryConfiguration Parse(JsonNode? node, IReadOnlyList<FileInfo> sourceFiles)
    {
        var obj = node.ExpectObject();

        var component = obj.TryGetNonNullPropertyValue(FieldNames.Component, out var componentNode)
            ? ComponentConfiguration.Parse(componentNode.ExpectObject())
            : null;

        var project = obj.TryGetNonNullPropertyValue(FieldNames.Project, out var projectNode)
            ? ProjectConfiguration.Parse(projectNode.ExpectObject())
            : null;

        return new RepositoryConfiguration(project, component, sourceFiles);
    }

    public RepositoryConfiguration Merge(RepositoryConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var project = Project?.Merge(other.Project) ?? other.Project;

        var component = Component?.Merge(other.Component) ?? other.Component;

        var sourceFiles = SourceFiles.Concat(other.SourceFiles).Distinct().ToArray();

        return new RepositoryConfiguration(project, component, sourceFiles);
    }

    public static RepositoryConfiguration? LoadFromFiles(IEnumerable<FileInfo> files)
    {
        var confixRc = files.FirstOrDefault(x => x.Name == FileNames.ConfixRepository);
        if (confixRc is null)
        {
            return null;
        }

        var json = JsonNode.Parse(File.ReadAllText(confixRc.FullName));
        return Parse(json, new[] { confixRc });
    }
}
