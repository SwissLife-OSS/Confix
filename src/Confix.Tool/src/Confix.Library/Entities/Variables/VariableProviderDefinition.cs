using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class VariableProviderDefinition
{
    public VariableProviderDefinition(
        string name,
        string type,
        IReadOnlyDictionary<string, JsonObject> environmentOverrides,
        JsonObject value)
    {
        Name = name;
        Type = type;
        EnvironmentOverrides = environmentOverrides;
        Value = value;
    }

    public string Name { get; }

    public string Type { get; }

    public IReadOnlyDictionary<string, JsonObject> EnvironmentOverrides { get; }

    public JsonObject Value { get; }

    public void WriteTo(Utf8JsonWriter writer) => Value.WriteTo(writer);

    public static VariableProviderDefinition From(VariableProviderConfiguration configuration)
    {
        List<string> validationErrors = new();
        if (configuration.Name is null)
        {
            validationErrors.Add("Provider name is required.");
        }

        if (configuration.Type is null)
        {
            validationErrors.Add("Provider type is required.");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Variable provider configuration is invalid.")
            {
                Errors = validationErrors
            };
        }

        return new VariableProviderDefinition(
            configuration.Name!,
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
