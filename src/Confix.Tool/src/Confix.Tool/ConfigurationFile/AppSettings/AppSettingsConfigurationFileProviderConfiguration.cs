using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ConfiX.Extensions;

namespace Confix.ConfigurationFiles;

public class AppSettingsConfigurationFileProviderConfiguration
{
    [JsonPropertyName("useUserSecrets")]
    public bool? UseUserSecrets { get; init; }

    public static AppSettingsConfigurationFileProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(
                JsonSerialization.Default.AppSettingsConfigurationFileProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Configuration of appsettings configuration file provider is invalid",
                ex);
        }
    }
}