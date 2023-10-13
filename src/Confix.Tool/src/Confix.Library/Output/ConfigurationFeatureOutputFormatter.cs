using System.Buffers;
using System.Text;
using System.Text.Json;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Configuration;

public sealed class ConfigurationFeatureOutputFormatter : IOutputFormatter<ConfigurationFeature>
{
    /// <inheritdoc />
    public bool CanHandle(OutputFormat format, ConfigurationFeature value)
        => format == OutputFormat.Json;

    /// <inheritdoc />
    public Task<string> FormatAsync(
        OutputFormat format,
        ConfigurationFeature value)
    {
        var options = new JsonWriterOptions
        {
            Indented = true
        };

        var buffer = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(buffer, options);
        value.WriteTo(writer);
        writer.Flush();

        return Task.FromResult(Encoding.UTF8.GetString(buffer.WrittenSpan));
    }
}
