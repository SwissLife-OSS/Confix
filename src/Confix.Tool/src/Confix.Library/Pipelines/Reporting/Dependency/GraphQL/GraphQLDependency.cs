using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;

namespace Confix.Tool.Reporting;

public sealed record GraphQLDependency(
    string Type,
    string Kind,
    string Path,
    JsonNode Value) : IDependency
{
    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("kind", Kind);
        writer.WriteString("path", Path);
        writer.WritePropertyName("data");
        Value.WriteTo(writer);

        writer.WriteEndObject();
    }
}
