using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class VariableProviderConfiguration
{
    private static class FieldNames
    {
        public const string Name = "name";
        public const string Type = "type";
        public const string EnvironmentOverride = "environmentOverride";
    }

    public VariableProviderConfiguration(
        string? name,
        string? type,
        IReadOnlyDictionary<string, JsonObject>? environmentOverrides,
        JsonObject values)
    {
        Name = name;
        Type = type;
        EnvironmentOverrides = environmentOverrides;
        Values = values;
    }

    public string? Name { get; }

    public string? Type { get; }

    public IReadOnlyDictionary<string, JsonObject>? EnvironmentOverrides { get; }

    public JsonObject Values { get; }

    public static VariableProviderConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        var environmentOverrides = obj
            .TryGetNonNullPropertyValue(FieldNames.EnvironmentOverride, out var envOverridesNode)
            ? envOverridesNode
                .ExpectObject()
                .ToDictionary(
                    property => property.Key,
                    property => property.Value.ExpectObject())
            : null;

        return new VariableProviderConfiguration(name, type, environmentOverrides, obj);
    }

    public VariableProviderConfiguration Merge(VariableProviderConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var name = other.Name ?? Name;

        var type = other.Type ?? Type;

        var environmentOverrides = (EnvironmentOverrides, other.EnvironmentOverrides)
            .MergeWith(
                (x, y) => x.Key == y.Key,
                (x, y) => new(x.Key ?? y.Key, x.Value.Merge(y.Value)!.AsObject()))
            ?.ToDictionary(x => x.Key, x => x.Value);

        var values = Values.Merge(other.Values)!.AsObject();

        return new VariableProviderConfiguration(name, type, environmentOverrides, values);
    }
}
