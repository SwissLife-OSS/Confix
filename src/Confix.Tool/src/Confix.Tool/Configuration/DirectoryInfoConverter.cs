using System.Text.Json;
using System.Text.Json.Serialization;

namespace Confix.Extensions;

public sealed class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
{
    public override DirectoryInfo Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => new(reader.GetString()!);

    public override void Write(
        Utf8JsonWriter writer,
        DirectoryInfo info,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(info.ToString());
}
