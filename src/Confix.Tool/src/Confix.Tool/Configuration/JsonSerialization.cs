using System.Text.Json.Serialization;
using Confix.Tool.Abstractions;
using Confix.Tool.Entities.VsCode;

namespace ConfiX.Extensions;

[JsonSerializable(typeof(ComponentSettingsFile))]
[JsonSerializable(typeof(VsCodeConfig))]
public partial class JsonSerialization : JsonSerializerContext
{
}
