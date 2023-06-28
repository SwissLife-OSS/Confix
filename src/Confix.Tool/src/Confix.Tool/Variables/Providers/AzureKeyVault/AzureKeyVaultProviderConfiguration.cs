using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record AzureKeyVaultProviderConfiguration(
    string? Uri
)
{
    public static AzureKeyVaultProviderConfiguration Parse(JsonNode node)
        => node.Deserialize(JsonSerialization.Instance.AzureKeyVaultProviderConfiguration)!;
}
