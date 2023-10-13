using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Extensions;

namespace Confix.ConfigurationFiles;

public sealed record AppSettingsConfigurationFileProviderConfiguration(bool? UseUserSecrets)
{
    public static AppSettingsConfigurationFileProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(
                JsonSerialization.Instance.AppSettingsConfigurationFileProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Configuration of appsettings configuration file provider is invalid",
                ex);
        }
    }
}
