using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Schema;
using Confix.Utilities.Json;
using Microsoft.Extensions.Logging;

namespace ConfiX.Extensions;

public sealed class RuntimeConfiguration
{
    private static class FieldNames
    {
        public const string IsRoot = "isRoot";
        public const string Component = "component";
        public const string Project = "project";
    }

    public RuntimeConfiguration(
        bool isRoot,
        ProjectConfiguration? project,
        ComponentConfiguration? component,
        IReadOnlyList<FileInfo> sourceFiles)
    {
        IsRoot = isRoot;
        Project = project;
        Component = component;
        SourceFiles = sourceFiles;
    }

    public bool IsRoot { get; set; }

    public ComponentConfiguration? Component { get; }

    public ProjectConfiguration? Project { get; }

    public IReadOnlyList<FileInfo> SourceFiles { get; }

    public static RuntimeConfiguration Parse(JsonNode? node)
    {
        return Parse(node, Array.Empty<FileInfo>());
    }

    public static RuntimeConfiguration Parse(JsonNode? node, IReadOnlyList<FileInfo> sourceFiles)
    {
        var obj = node.ExpectObject();

        var isRoot = !obj.TryGetNonNullPropertyValue(FieldNames.IsRoot, out var isRootNode) ||
            isRootNode.ExpectValue<bool>();

        var component = obj.TryGetNonNullPropertyValue(FieldNames.Component, out var componentNode)
            ? ComponentConfiguration.Parse(componentNode.ExpectObject())
            : null;

        var project = obj.TryGetNonNullPropertyValue(FieldNames.Project, out var projectNode)
            ? ProjectConfiguration.Parse(projectNode.ExpectObject())
            : null;

        return new RuntimeConfiguration(isRoot, project, component, sourceFiles);
    }

    public RuntimeConfiguration Merge(RuntimeConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var isRoot = other.IsRoot || IsRoot;

        var project = Project?.Merge(other.Project) ?? other.Project;

        var component = Component?.Merge(other.Component) ?? other.Component;

        var sourceFiles = SourceFiles.Concat(other.SourceFiles).ToArray();

        return new RuntimeConfiguration(isRoot, project, component, sourceFiles);
    }

    public static RuntimeConfiguration LoadFromFiles(IEnumerable<FileInfo> files)
    {
        var config = new RuntimeConfiguration(true, null, null, Array.Empty<FileInfo>());

        foreach (var file in files.Where(x => x.Name == FileNames.ConfixRc))
        {
            var json = JsonNode.Parse(File.ReadAllText(file.FullName));
            var innerConfig = Parse(json, new[] { file });
            if (innerConfig.IsRoot)
            {
                config = innerConfig;
            }
            else
            {
                config = config.Merge(innerConfig);
            }
        }

        return config;
    }
}