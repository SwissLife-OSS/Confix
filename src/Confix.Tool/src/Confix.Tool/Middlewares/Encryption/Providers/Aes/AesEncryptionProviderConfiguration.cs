using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace Confix.Tool.Middlewares.Encryption.Providers.Aes;

public sealed record AesEncryptionProviderConfiguration(
    string? Key,
    string? IV
)
{
    public static AesEncryptionProviderConfiguration Parse(JsonNode node)
      => node.Deserialize(JsonSerialization.Instance.AesEncryptionProviderConfiguration)!;
}
