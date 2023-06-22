using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record AzureKeyVaultProviderConfiguration(
    string Uri
)
{
    public static AzureKeyVaultProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(JsonSerialization.Instance.AzureKeyVaultProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Configuration of AzureKeyVaultProviderConfiguration is invalid",
                ex);
        }
    }
}
