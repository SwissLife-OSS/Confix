using System.Collections.Immutable;
using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class EncryptionProviderDefinition
{
    public EncryptionProviderDefinition(
        string type,
        IReadOnlyDictionary<string, JsonObject> environmentOverrides,
        JsonObject value)
    {
        Type = type;
        EnvironmentOverrides = environmentOverrides;
        Value = value;
    }

    public string Type { get; }

    public IReadOnlyDictionary<string, JsonObject> EnvironmentOverrides { get; }

    public JsonObject Value { get; }

    public static EncryptionProviderDefinition From(EncryptionProviderConfiguration configuration)
    {
        if (configuration.Type is null)
        {
            throw new ValidationException("Encryption provider configuration is invalid.")
            {
                Errors = new[] { "Provider type is required." }
            };
        }

        return new EncryptionProviderDefinition(
            configuration.Type!,
            configuration.EnvironmentOverrides ?? ImmutableDictionary<string, JsonObject>.Empty,
            configuration.Values);
    }

    public JsonNode ValueWithOverrides(string environmentName)
    {
        if (EnvironmentOverrides.GetValueOrDefault(environmentName) is { } envOverride)
        {
            return Value.Merge(envOverride)!;
        }
        return Value;
    }
}
