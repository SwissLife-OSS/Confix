using System.Text.Json;
using System.Text.Json.Serialization;
using Confix.ConfigurationFiles;
using ConfiX.Variables;

namespace ConfiX.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LocalVariableProviderConfiguration))]
[JsonSerializable(typeof(AzureKeyVaultProviderConfiguration))]
[JsonSerializable(typeof(SecretVariableProviderConfiguration))]
[JsonSerializable(typeof(GitVariableProviderConfiguration))]
[JsonSerializable(typeof(AppSettingsConfigurationFileProviderConfiguration))]
public partial class JsonSerialization : JsonSerializerContext
{
    public static JsonSerialization Enum { get; } = new(new JsonSerializerOptions
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    });
}
