using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public record SecretVariableProviderConfiguration(
    SecretVariableProviderAlgorithm? Algorithm,
    EncryptionPadding? Padding,
    string? PublicKey,
    string? PublicKeyPath,
    string? PrivateKey,
    string? PrivateKeyPath,
    string? Password
)
{
    public static SecretVariableProviderConfiguration Parse(JsonNode node)
        => node.Deserialize(JsonSerialization.Instance.SecretVariableProviderConfiguration)!;
}
