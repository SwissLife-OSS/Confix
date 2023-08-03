using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Extensions;

namespace Confix.Variables;

public sealed record LocalVariableProviderConfiguration(
  string? Path
)
{
    public static LocalVariableProviderConfiguration Parse(JsonNode node)
        => node.Deserialize(JsonSerialization.Instance.LocalVariableProviderConfiguration)!;
}
