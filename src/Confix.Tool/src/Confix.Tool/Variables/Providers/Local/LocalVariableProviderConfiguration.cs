using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record LocalVariableProviderConfiguration(
  string Path
)
{
    public static LocalVariableProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(JsonSerialization.Default.LocalVariableProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Configuration of LocalVariableProvider is invalid", ex);
        }
    }
}
