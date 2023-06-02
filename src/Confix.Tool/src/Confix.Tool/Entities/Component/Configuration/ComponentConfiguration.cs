using System.Text.Json.Nodes;
using Confix.Tool.Schema;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class ComponentConfiguration
{
    private static class FieldNames
    {
        public const string Name = "name";
        public const string Inputs = "inputs";
        public const string Outputs = "outputs";
    }

    public ComponentConfiguration(
        string? name,
        IReadOnlyList<ComponentInputConfiguration>? inputs,
        IReadOnlyList<ComponentOutputConfiguration>? outputs,
        IReadOnlyList<FileInfo> sourceFiles)
    {
        Name = name;
        Inputs = inputs;
        Outputs = outputs;
        SourceFiles = sourceFiles;
    }

    public string? Name { get; }

    public IReadOnlyList<ComponentInputConfiguration>? Inputs { get; }

    public IReadOnlyList<ComponentOutputConfiguration>? Outputs { get; }

    public IReadOnlyList<FileInfo> SourceFiles { get; }

    public static ComponentConfiguration Parse(JsonNode? node)
    {
        return Parse(node, Array.Empty<FileInfo>());
    }

    public static ComponentConfiguration Parse(JsonNode? node, IReadOnlyList<FileInfo> sourceFiles)
    {
        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var inputs = obj
            .MaybeProperty(FieldNames.Inputs)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(ComponentInputConfiguration.Parse)
            .ToArray();

        var outputs = obj
            .MaybeProperty(FieldNames.Outputs)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(ComponentOutputConfiguration.Parse)
            .ToArray();

        return new ComponentConfiguration(name, inputs, outputs, sourceFiles);
    }

    public ComponentConfiguration Merge(ComponentConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var name = other.Name ?? Name;

        var inputs = (Inputs, other.Inputs)
            .MergeWith((x, y) => x.Type == y.Type, (x, y) => x.Merge(y));

        var outputs = (Outputs, other.Outputs)
            .MergeWith((x, y) => x.Type == y.Type, (x, y) => x.Merge(y));

        var sourceFiles = SourceFiles.Concat(other.SourceFiles).Distinct().ToArray();

        return new ComponentConfiguration(name, inputs, outputs, sourceFiles);
    }

    public static ComponentConfiguration? LoadFromFiles(IEnumerable<FileInfo> files)
    {
        var config = files.FirstOrDefault(x => x.Name == FileNames.ConfixComponent);
        if (config is null)
        {
            return null;
        }

        var json = JsonNode.Parse(File.ReadAllText(config.FullName));
        return Parse(json, new[] { config });
    }
}
