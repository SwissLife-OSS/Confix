using System.Text.Json.Nodes;
using Confix.Utilities.Json;
using Json.Schema;

namespace Confix.Tool.Abstractions;

public sealed class EnvironmentConfiguration
{
    private static class FieldNames
    {
        public const string Name = "name";
        public const string ExcludeFiles = "excludeFiles";
        public const string IncludeFiles = "includeFiles";
        public const string Enabled = "enabled";
    }

    public EnvironmentConfiguration(
        string? name,
        IReadOnlyList<string>? excludeFiles,
        IReadOnlyList<string>? includeFiles,
        bool? enabled)
    {
        Name = name;
        ExcludeFiles = excludeFiles;
        IncludeFiles = includeFiles;
        Enabled = enabled;
    }

    public string? Name { get; }

    public IReadOnlyList<string>? ExcludeFiles { get; }
    public IReadOnlyList<string>? IncludeFiles { get; }
    public bool? Enabled { get; }

    public static EnvironmentConfiguration Parse(JsonNode node)
    {
        if (node.GetSchemaValueType() is SchemaValueType.String)
        {
            return new EnvironmentConfiguration(node.ExpectValue<string>(), null, null, null);
        }

        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var excludeFiles =
            obj.TryGetNonNullPropertyValue(FieldNames.ExcludeFiles, out var excludeFilesNode)
                ? excludeFilesNode
                    .ExpectArray()
                    .WhereNotNull()
                    .Select(n => n.ExpectValue<string>())
                    .ToArray()
                : null;

        var includeFiles =
            obj.TryGetNonNullPropertyValue(FieldNames.IncludeFiles, out var includeFilesNode)
                ? includeFilesNode
                    .ExpectArray()
                    .WhereNotNull()
                    .Select(n => n.ExpectValue<string>())
                    .ToArray()
                : null;

        var enabled = obj.MaybeProperty(FieldNames.Enabled)?.ExpectValue<bool>();

        return new EnvironmentConfiguration(name, excludeFiles, includeFiles, enabled);
    }

    public EnvironmentConfiguration Merge(EnvironmentConfiguration other)
    {
        var name = other.Name ?? Name;
        var excludeFiles = other.ExcludeFiles ?? ExcludeFiles;
        var includeFiles = other.IncludeFiles ?? IncludeFiles;
        var enabled = other.Enabled ?? Enabled;

        return new EnvironmentConfiguration(name, excludeFiles, includeFiles, enabled);
    }
}
