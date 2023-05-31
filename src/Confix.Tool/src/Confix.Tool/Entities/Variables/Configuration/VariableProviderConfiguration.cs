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
        public const string Path = "path";
    }

    public VariableProviderConfiguration(
        string? name,
        string? type,
        IReadOnlyDictionary<string, JsonObject> environmentOverrides,
        JsonObject value)
    {
        Name = name;
        Type = type;
        EnvironmentOverrides = environmentOverrides;
        Value = value;
    }

    public string? Name { get; }

    public string? Type { get; }

    public IReadOnlyDictionary<string, JsonObject> EnvironmentOverrides { get; }

    public JsonObject Value { get; }

    public static VariableProviderConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        var environmentOverrides =
            obj.TryGetPropertyValue(FieldNames.EnvironmentOverride, out var environmentOverrideNode)
                ? environmentOverrideNode
                    .ExpectObject()
                    .ToDictionary(
                        property => property.Key,
                        property => property.Value.ExpectObject())
                : new Dictionary<string, JsonObject>();

        return new VariableProviderConfiguration(name, type, environmentOverrides, obj);
    }
}
