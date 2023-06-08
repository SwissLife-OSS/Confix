using System.Text.Json.Serialization;
using Confix.Tool.Abstractions;
using Confix.Tool.Entities.VsCode;
using ConfiX.Variables;

namespace ConfiX.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(VsCodeConfig))]
[JsonSerializable(typeof(LocalVariableProviderConfiguration))]
[JsonSerializable(typeof(AzureKeyVaultProviderConfiguration))]
public partial class JsonSerialization : JsonSerializerContext
{
}
