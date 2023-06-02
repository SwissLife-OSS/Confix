using System.Text.Json.Serialization;
using Confix.Tool.Abstractions;
using Confix.Tool.Entities.VsCode;
using ConfiX.Variables;

namespace ConfiX.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(VsCodeConfig))]
[JsonSerializable(typeof(LocalVariableProviderConfiguration))]
public partial class JsonSerialization : JsonSerializerContext
{
}
