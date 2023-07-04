using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;

public sealed record AzureKeyVaultEncryptionProviderConfiguration(
    string? Uri,
    string? KeyName,
    string? KeyVersion
)
{
    public static AzureKeyVaultEncryptionProviderConfiguration Parse(JsonNode node)
      => node.Deserialize(JsonSerialization.Instance.AzureKeyVaultEncryptionProviderConfiguration)!;
}
