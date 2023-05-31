using System.Text.Json;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public record VariableProviderConfiguration
{
    public required string Name { get; init; }
    public required string Type { get; init; }

    /// <summary>
    /// Hold Overrides for the current environment
    /// </summary>
    public required JsonNode Configuration { get; init; }
};