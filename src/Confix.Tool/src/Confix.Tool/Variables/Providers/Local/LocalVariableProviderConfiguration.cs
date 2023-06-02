using System.Text.Json;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public sealed record LocalVariableProviderConfiguration
{
    public required string FilePath { get; init; }
    public static LocalVariableProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize<LocalVariableProviderConfiguration>()!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Configuration of LocalVariableProvider is invalid", 
                ex);
        }
    }
}
