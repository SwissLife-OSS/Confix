using System.Text.Json.Nodes;
using Confix.Utilities.Json;
using Json.Schema;

namespace Confix.Tool.Abstractions;

public sealed class EnvironmentConfiguration
{
    public static class FieldNames
    {
        public const string Name = "name";
        public const string Enabled = "enabled";
    }

    public EnvironmentConfiguration(
        string? name,
        bool? enabled)
    {
        Name = name;
        Enabled = enabled;
    }

    public string? Name { get; }
    public bool? Enabled { get; }

    public static EnvironmentConfiguration Parse(JsonNode node)
    {
        if (node.GetSchemaValueType() is SchemaValueType.String)
        {
            return new EnvironmentConfiguration(node.ExpectValue<string>(), null);
        }

        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var enabled = obj.MaybeProperty(FieldNames.Enabled)?.ExpectValue<bool>();

        return new EnvironmentConfiguration(name, enabled);
    }

    public EnvironmentConfiguration Merge(EnvironmentConfiguration other)
    {
        var name = other.Name ?? Name;
        var enabled = other.Enabled ?? Enabled;

        return new EnvironmentConfiguration(name, enabled);
    }
}
