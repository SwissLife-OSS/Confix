using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record LocalVariableProviderConfiguration(
  string? Path
)
{
    public static LocalVariableProviderConfiguration Parse(JsonNode node)
        => node.Deserialize(JsonSerialization.Instance.LocalVariableProviderConfiguration)!;
}
