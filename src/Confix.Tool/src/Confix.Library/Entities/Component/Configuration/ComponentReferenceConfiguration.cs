using System.Text.Json.Nodes;
using Confix.Utilities.Json;
using Confix.Utilities.Parsing;
using Json.Schema;
using static System.StringSplitOptions;

namespace Confix.Tool.Abstractions;

public sealed class ComponentReferenceConfiguration
{
    public static class FieldNames
    {
        public const string MountingPoint = "mountingPoint";
        public const string Version = "version";
    }

    public ComponentReferenceConfiguration(
        string? provider,
        string? componentName,
        string? version,
        bool isEnabled,
        IReadOnlyList<string>? mountingPoints)
    {
        Provider = provider;
        ComponentName = componentName;
        Version = version;
        MountingPoints = mountingPoints;
        IsEnabled = isEnabled;
    }

    public string? Provider { get; }

    public string? ComponentName { get; }

    public string? Version { get; }

    public bool IsEnabled { get; }

    public IReadOnlyList<string>? MountingPoints { get; }

    public static ComponentReferenceConfiguration Parse(string key, JsonNode node)
    {
        if (key.Split("/", TrimEntries | RemoveEmptyEntries) is not
            [['@', .. var provider], var componentName])
        {
            throw new JsonParseException(
                node,
                "The component key must be in the format '@provider/componentName'.");
        }

        if (node.GetSchemaValueType() is SchemaValueType.String)
        {
            return new ComponentReferenceConfiguration(
                provider,
                componentName,
                node.ExpectValue<string>(),
                true,
                null);
        }

        if (node.GetSchemaValueType() is SchemaValueType.Boolean)
        {
            return new ComponentReferenceConfiguration(
                provider,
                componentName,
                null,
                node.ExpectValue<bool>(),
                null);
        }

        var obj = node.ExpectObject();

        var version = obj.TryGetNonNullPropertyValue(FieldNames.Version, out var versionNode)
            ? versionNode.ExpectValue<string>()
            : null;

        var mountingPoints =
            obj.TryGetNonNullPropertyValue(FieldNames.MountingPoint, out var mountingPointNode)
                ? mountingPointNode.GetSchemaValueType() is SchemaValueType.Array
                    ? mountingPointNode
                        .ExpectArray()
                        .WhereNotNull()
                        .Select(n => n.ExpectValue<string>())
                        .ToArray()
                    : new[] { mountingPointNode.ExpectValue<string>() }
                : null;

        return new ComponentReferenceConfiguration(
            provider,
            componentName,
            version,
            true,
            mountingPoints);
    }

    public ComponentReferenceConfiguration Merge(ComponentReferenceConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var provider = other.Provider;
        var componentName = other.ComponentName;
        var version = other.Version ?? Version;
        var isEnabled = other.IsEnabled;
        var mountingPoint = other.MountingPoints ?? MountingPoints;

        return new ComponentReferenceConfiguration(provider,
            componentName,
            version,
            isEnabled,
            mountingPoint);
    }
}
