using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public record SecretVariableProviderConfiguration
{
    [DefaultValue(SecretVariableProviderAlgorithm.RSA)]
    public SecretVariableProviderAlgorithm Algorithm { get; init; }

    [DefaultValue(EncryptionPadding.OaepSHA256)]
    public EncryptionPadding Padding { get; init; }

    public string? PublicKey { get; init; }

    public string? PublicKeyPath { get; init; }

    public string? PrivateKey { get; init; }

    public string? PrivateKeyPath { get; init; }

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
