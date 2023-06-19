using System.Text.Json.Serialization;
using ConfiX.Variables;

namespace ConfiX.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LocalVariableProviderConfiguration))]
[JsonSerializable(typeof(AzureKeyVaultProviderConfiguration))]
[JsonSerializable(typeof(SecretVariableProviderConfiguration))]
[JsonSerializable(typeof(GitVariableProviderConfiguration))]
public partial class JsonSerialization : JsonSerializerContext
{
}
