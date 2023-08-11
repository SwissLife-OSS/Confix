using System.Buffers;
using System.CommandLine.Invocation;
using System.Text;
using System.Text.Json;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Configuration;

public sealed class ConfigurationFileOutputFormatter
    : IOutputFormatter<IConfigurationFileCollection>
{
    /// <inheritdoc />
    public bool CanHandle(OutputFormat format, IConfigurationFileCollection value)
        => format == OutputFormat.Json;

    /// <inheritdoc />
    public Task<string> FormatAsync(
        InvocationContext context,
        OutputFormat format,
        IConfigurationFileCollection value)
    {
        var options = new JsonWriterOptions
        {
            Indented = true
        };

        var buffer = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(buffer, options);
        writer.WriteStartObject();
        writer.WriteStartArray("files");
        foreach (var file in value)
        {
            writer.WriteStartObject();
            writer.WriteString("path", file.File.FullName);
            writer.WriteString("kind", file.File.Name);
            writer.WritePropertyName("content");
            file.Content.WriteTo(writer);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        return Task.FromResult(Encoding.UTF8.GetString(buffer.WrittenSpan));
    }
}
