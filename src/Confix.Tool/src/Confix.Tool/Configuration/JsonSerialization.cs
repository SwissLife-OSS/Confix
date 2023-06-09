using System.Text.Json.Serialization;
using ConfiX.Variables;

namespace ConfiX.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LocalVariableProviderConfiguration))]
public partial class JsonSerialization : JsonSerializerContext
{
}
