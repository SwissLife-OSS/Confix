using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public record SecretVariableProviderConfiguration(
    [property:DefaultValue(SecretVariableProviderAlgorithm.RSA)]
    SecretVariableProviderAlgorithm Algorithm,
    [property:DefaultValue(EncryptionPadding.OaepSHA256)]
    EncryptionPadding Padding,
    string? PublicKey,
    string? PublicKeyPath,
    string? PrivateKey,
    string? PrivateKeyPath
)
{
    public static SecretVariableProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(JsonSerialization.Instance.SecretVariableProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Configuration of SecretVariableProvider is invalid", ex);
        }
    }
}
