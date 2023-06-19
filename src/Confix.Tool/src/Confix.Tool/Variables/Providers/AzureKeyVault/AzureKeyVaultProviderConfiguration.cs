using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record AzureKeyVaultProviderConfiguration
{
    [JsonPropertyName("Uri")]
    public required string Uri { get; init; }

    public static AzureKeyVaultProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(JsonSerialization.Default.AzureKeyVaultProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Configuration of AzureKeyVaultProviderConfiguration is invalid",
                ex);
        }
    }
}
