using System.Buffers;
using System.Text;
using System.Text.Json;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Reporting;

namespace Confix.Tool.Commands.Configuration;

public sealed class ReportListOutputFormatter
    : IOutputFormatter<IEnumerable<Report>>
{
    /// <inheritdoc />
    public bool CanHandle(OutputFormat format, IEnumerable<Report> value)
        => format == OutputFormat.Json;

    /// <inheritdoc />
    public Task<string> FormatAsync(
        OutputFormat format,
        IEnumerable<Report> value)
    {
        var options = new JsonWriterOptions
        {
            Indented = true
        };

        var buffer = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(buffer, options);
        writer.WriteStartArray();

        foreach (var report in value)
        {
            report.WriteTo(writer);
        }

        writer.WriteEndArray();
        writer.Flush();

        return Task.FromResult(Encoding.UTF8.GetString(buffer.WrittenSpan));
    }
}

file static class Extensions
{
    public static void WriteTo(this Report value, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("configurationPath", value.ConfigurationPath);
        writer.WriteString("environment", value.Environment);
        writer.WriteString("timestamp", value.Timestamp.ToString("O"));
        writer.WritePropertyName("project");
        value.Project.WriteTo(writer);
        writer.WritePropertyName("solution");
        value.Solution?.WriteTo(writer);
        writer.WritePropertyName("repository");
        value.Repository.WriteTo(writer);
        writer.WritePropertyName("commit");
        value.Commit.WriteTo(writer);

        writer.WriteEndObject();
    }

    private static void WriteTo(this ProjectReport value, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("name", value.Name);
        writer.WriteString("path", value.Path);

        writer.WriteEndObject();
    }

    private static void WriteTo(this SolutionReport value, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("name", value.Name);
        writer.WriteString("path", value.Path);

        writer.WriteEndObject();
    }

    private static void WriteTo(this RepositoryReport value, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("name", value.Name.ToString());
        writer.WriteString("originUrl", value.OriginUrl);

        writer.WriteEndObject();
    }

    private static void WriteTo(this CommitReport value, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("hash", value.Hash);
        writer.WriteString("message", value.Message);
        writer.WriteString("author", value.Author);
        writer.WriteString("email", value.Email);
        writer.WriteString("branch", value.Branch);
        writer.WritePropertyName("tags");
        writer.WriteStartArray();
        foreach (var tag in value.Tags)
        {
            writer.WriteStringValue(tag);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
