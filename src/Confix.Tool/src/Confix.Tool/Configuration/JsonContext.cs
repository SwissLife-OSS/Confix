using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Confix.ConfigurationFiles;
using Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;
using ConfiX.Variables;

namespace ConfiX.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LocalVariableProviderConfiguration))]
[JsonSerializable(typeof(AzureKeyVaultProviderConfiguration))]
[JsonSerializable(typeof(SecretVariableProviderConfiguration))]
[JsonSerializable(typeof(GitVariableProviderConfiguration))]
[JsonSerializable(typeof(AppSettingsConfigurationFileProviderConfiguration))]
[JsonSerializable(typeof(AzureKeyVaultEncryptionProviderConfiguration))]
[JsonSerializable(typeof(AesEncryptionProviderConfiguration))]
public partial class JsonSerialization : JsonSerializerContext
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static JsonSerialization Instance { get; } = new(options);
}