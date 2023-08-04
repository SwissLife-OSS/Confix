using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class EncryptionProviderConfiguration
{
    private static class FieldNames
    {
        public const string Type = "type";
        public const string EnvironmentOverride = "environmentOverride";
    }

    public EncryptionProviderConfiguration(
        string? type,
        IReadOnlyDictionary<string, JsonObject>? environmentOverrides,
        JsonObject values)
    {
        Type = type;
        EnvironmentOverrides = environmentOverrides;
        Values = values;
    }

    public string? Type { get; }

    public IReadOnlyDictionary<string, JsonObject>? EnvironmentOverrides { get; }

    public JsonObject Values { get; }

    public static EncryptionProviderConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        var environmentOverrides = obj
            .TryGetNonNullPropertyValue(FieldNames.EnvironmentOverride, out var envOverridesNode)
            ? envOverridesNode
                .ExpectObject()
                .ToDictionary(
                    property => property.Key,
                    property => property.Value.ExpectObject())
            : null;

        return new EncryptionProviderConfiguration(type, environmentOverrides, obj);
    }

    public EncryptionProviderConfiguration Merge(EncryptionProviderConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var type = other.Type ?? Type;

        var environmentOverrides = (EnvironmentOverrides, other.EnvironmentOverrides)
            .MergeWith(
                (x, y) => x.Key == y.Key,
                (x, y) => new(x.Key ?? y.Key, x.Value.Merge(y.Value)!.AsObject()))
            ?.ToDictionary(x => x.Key, x => x.Value);

        var values = Values.Merge(other.Values)!.AsObject();

        return new EncryptionProviderConfiguration(type, environmentOverrides, values);
    }
}
