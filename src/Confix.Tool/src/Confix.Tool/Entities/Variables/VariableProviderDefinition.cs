using System.Collections.Immutable;
using System.Text.Json.Nodes;

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

    public static VariableProviderDefinition From(VariableProviderConfiguration configuration)
    {
        var name = configuration.Name ??
            throw new InvalidOperationException("Variable provider name is required.");

        var type = configuration.Type ??
            throw new InvalidOperationException("Variable provider type is required.");

        var environmentOverrides = configuration.EnvironmentOverrides
            ?? ImmutableDictionary<string, JsonObject>.Empty;

        var value = configuration.Values;

        return new VariableProviderDefinition(name, type, environmentOverrides, value);
    }
}
