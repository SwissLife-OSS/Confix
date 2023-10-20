using System.Text.Json;
using System.Text.RegularExpressions;

namespace Confix.Tool.Reporting;

public sealed record RegexDependency(
    string Type,
    string Kind,
    string Path,
    GroupCollection Groups) : IDependency
{
    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("kind", Kind);
        writer.WriteString("path", Path);

        writer.WriteStartArray("data");
        foreach (var group in Groups.Keys)
        {
            if (int.TryParse(group, out _))
            {
                continue;
            }

            writer.WriteStartObject();
            writer.WriteString("name", group);
            writer.WriteString("value", Groups[group].Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
