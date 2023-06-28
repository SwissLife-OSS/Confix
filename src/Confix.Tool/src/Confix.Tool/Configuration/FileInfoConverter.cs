using System.Text.Json;
using System.Text.Json.Serialization;

namespace Confix.Extensions;

public sealed class FileInfoConverter : JsonConverter<FileInfo>
{
    public override FileInfo Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => new(reader.GetString()!);

    public override void Write(
        Utf8JsonWriter writer,
        FileInfo info,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(info.ToString());
}
