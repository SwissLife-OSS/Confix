using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public record SecretVariableProviderConfiguration
{
    [DefaultValue(SecretVariableProviderAlgorithm.RSA)]
    [JsonPropertyName("algorithm")]
    public required SecretVariableProviderAlgorithm Algorithm { get; init; }

    [JsonPropertyName("publicKey")]
    public string? PublicKey { get; init; }

    [JsonPropertyName("publicKeyPath")]
    public string? PublicKeyPath { get; init; }

    [JsonPropertyName("privateKey")]
    public string? PrivateKey { get; init; }

    [JsonPropertyName("privateKeyPath")]
    public string? PrivateKeyPath { get; init; }

    public static SecretVariableProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(JsonSerialization.Default.SecretVariableProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Configuration of SecretVariableProvider is invalid",
                ex);
        }
    }
}
