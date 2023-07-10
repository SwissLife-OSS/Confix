using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Middlewares;
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
        public const string Encryption = "encryption";
    }

    public RuntimeConfiguration(
        bool isRoot,
        ProjectConfiguration? project,
        ComponentConfiguration? component,
        EncryptionConfiguration? encryption,
        IReadOnlyList<JsonFile> sourceFiles)
    {
        IsRoot = isRoot;
        Project = project;
        Component = component;
        Encryption = encryption;
        SourceFiles = sourceFiles;
    }

    public bool IsRoot { get; set; }

    public ComponentConfiguration? Component { get; }

    public ProjectConfiguration? Project { get; }

    public EncryptionConfiguration? Encryption { get; }

    public IReadOnlyList<JsonFile> SourceFiles { get; }

    public static RuntimeConfiguration Parse(JsonNode? node)
    {
        return Parse(node, Array.Empty<JsonFile>());
    }

    public static RuntimeConfiguration Parse(JsonNode? node, IReadOnlyList<JsonFile> sourceFiles)
    {
        var obj = node.ExpectObject();

        var isRoot = obj.TryGetNonNullPropertyValue(FieldNames.IsRoot, out var isRootNode) &&
            isRootNode.ExpectValue<bool>();

        var component = obj.TryGetNonNullPropertyValue(FieldNames.Component, out var componentNode)
            ? ComponentConfiguration.Parse(componentNode.ExpectObject())
            : null;

        var project = obj.TryGetNonNullPropertyValue(FieldNames.Project, out var projectNode)
            ? ProjectConfiguration.Parse(projectNode.ExpectObject())
            : null;

        var encryption = obj.TryGetNonNullPropertyValue(FieldNames.Encryption, out var encryptionNode)
            ? EncryptionConfiguration.Parse(encryptionNode.ExpectObject())
            : null;

        return new RuntimeConfiguration(
            isRoot,
            project,
            component,
            encryption,
            sourceFiles);
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

        var encryption = Encryption?.Merge(other.Encryption) ?? other.Encryption;

        var sourceFiles = SourceFiles.Concat(other.SourceFiles).ToArray();

        return new RuntimeConfiguration(isRoot, project, component, encryption, sourceFiles);
    }

    public static RuntimeConfiguration LoadFromFiles(IEnumerable<JsonFile> files)
    {
        var config = new RuntimeConfiguration(true, null, null, null, Array.Empty<JsonFile>());

        foreach (var file in files.Where(x => x.File.Name == FileNames.ConfixRc))
        {
            var json = file.Content;
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