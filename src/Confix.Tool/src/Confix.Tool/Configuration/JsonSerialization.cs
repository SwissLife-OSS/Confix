using System.Text.Json.Serialization;
using ConfiX.Variables;

namespace ConfiX.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LocalVariableProviderConfiguration))]
[JsonSerializable(typeof(AzureKeyVaultProviderConfiguration))]
[JsonSerializable(typeof(SecretVariableProviderConfiguration))]
public partial class JsonSerialization : JsonSerializerContext
{
}
