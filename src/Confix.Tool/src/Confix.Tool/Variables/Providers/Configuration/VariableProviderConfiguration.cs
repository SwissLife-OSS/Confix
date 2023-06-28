using System.Text.Json.Nodes;

namespace Confix.Variables;

public sealed record VariableProviderConfiguration
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required JsonNode Configuration { get; init; }
}
