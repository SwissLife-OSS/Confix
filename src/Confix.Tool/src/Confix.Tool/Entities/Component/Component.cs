using System.Text.Json.Nodes;
using Confix.Utilities.Json;
using Confix.Utilities.Parsing;
using Json.Schema;

namespace Confix.Tool.Abstractions;

public sealed class Component
{
    private static class FieldNames
    {
        public const string MountingPoint = "mountingPoint";
        public const string Version = "version";
    }

    public Component(
        string provider,
        string componentName,
        string? version,
        bool isEnabled,
        IReadOnlyList<string>? mountingPoint)
    {
        Provider = provider;
        ComponentName = componentName;
        Version = version;
        MountingPoint = mountingPoint;
        IsEnabled = isEnabled;
    }

    public string Provider { get; }

    public string ComponentName { get; }

    public string? Version { get; }

    public bool IsEnabled { get; }

    public IReadOnlyList<string>? MountingPoint { get; }

    public static Component Parse(string key, JsonNode node)
    {
        if (key.Split("/") is not [['@', .. var provider], var componentName])
        {
            throw new JsonParseException(
                node,
                "The component key must be in the format '@provider/componentName'.");
        }

        if (node.GetSchemaValueType() is SchemaValueType.String)
        {
            return new Component(
                provider,
                componentName,
                node.ExpectValue<string>(),
                true,
                null);
        }

        if (node.GetSchemaValueType() is SchemaValueType.Boolean)
        {
            return new Component(provider, componentName, null, node.ExpectValue<bool>(), null);
        }

        var obj = node.ExpectObject();

        var version = obj.TryGetPropertyValue(FieldNames.Version, out var versionNode)
            ? versionNode.ExpectValue<string>()
            : null;

        var mountingPoints = obj[FieldNames.MountingPoint] is { } mountingPointNode
            ? mountingPointNode.GetSchemaValueType() is SchemaValueType.Array
                ? mountingPointNode
                    .ExpectArray()
                    .WhereNotNull()
                    .Select(n => n.ExpectValue<string>())
                    .ToArray()
                : new[] { mountingPointNode.ExpectValue<string>() }
            : Array.Empty<string>();

        return new Component(provider, componentName, version, true, mountingPoints);
    }
}
