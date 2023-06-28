using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Confix.ConfigurationFiles;
using Confix.Tool.Middlewares;
using Confix.Variables;
using Json.Schema;

namespace Confix.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LocalVariableProviderConfiguration))]
[JsonSerializable(typeof(AzureKeyVaultProviderConfiguration))]
[JsonSerializable(typeof(SecretVariableProviderConfiguration))]
[JsonSerializable(typeof(GitVariableProviderConfiguration))]
[JsonSerializable(typeof(AppSettingsConfigurationFileProviderConfiguration))]
[JsonSerializable(typeof(ConfigurationFeature))]
public partial class JsonSerialization : JsonSerializerContext
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumCamelCaseConverter(),
            new FileInfoConverter(),
            new DirectoryInfoConverter(),
            new TypeMappingConverter<IConfigurationFileCollection, ConfigurationFileCollection>()
        }
    };

    public static JsonSerialization Instance { get; } = new(options);
}