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
    }

    public EnvironmentConfiguration(string? name, IReadOnlyList<string> excludeFiles)
    {
        Name = name;
        ExcludeFiles = excludeFiles;
    }

    public string? Name { get; }

    public IReadOnlyList<string> ExcludeFiles { get; }

    public static EnvironmentConfiguration Parse(JsonNode node)
    {
        if (node.GetSchemaValueType() is SchemaValueType.String)
        {
            return new EnvironmentConfiguration(node.ExpectValue<string>(), Array.Empty<string>());
        }

        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var excludeFiles =
            obj.TryGetPropertyValue(FieldNames.ExcludeFiles, out var excludeFilesNode)
                ? excludeFilesNode
                    .ExpectArray()
                    .WhereNotNull()
                    .Select(n => n.ExpectValue<string>())
                    .ToArray()
                : Array.Empty<string>();

        return new EnvironmentConfiguration(name, excludeFiles);
    }
}
