using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Confix.Extensions;

namespace Confix.Variables;

public sealed record LocalVariableProviderConfiguration(
  [property: JsonRequired]string Path
)
{
    public static LocalVariableProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(JsonSerialization.Instance.LocalVariableProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Configuration of LocalVariableProvider is invalid", ex);
        }
    }
}
