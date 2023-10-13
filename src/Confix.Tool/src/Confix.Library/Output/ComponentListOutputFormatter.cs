using System.Buffers;
using System.Text;
using System.Text.Json;
using Confix.Tool.Commands.Logging;
using Json.More;

namespace Confix.Tool.Commands.Configuration;

public sealed class ComponentListOutputFormatter
    : IOutputFormatter<IEnumerable<Confix.Tool.Abstractions.Component>>
{
    /// <inheritdoc />
    public bool CanHandle(OutputFormat format, IEnumerable<Abstractions.Component> value)
        => format == OutputFormat.Json;

    /// <inheritdoc />
    public Task<string> FormatAsync(
        OutputFormat format,
        IEnumerable<Abstractions.Component> value)
    {
        var options = new JsonWriterOptions
        {
            Indented = true
        };

        var buffer = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(buffer, options);
        writer.WriteStartObject();
        writer.WritePropertyName("components");
        writer.WriteStartArray();

        foreach (var component in value)
        {
            component.WriteTo(writer);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        return Task.FromResult(Encoding.UTF8.GetString(buffer.WrittenSpan));
    }
}

file static class Extensions
{
    public static void WriteTo(this Abstractions.Component value, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("provider", value.Provider);
        writer.WriteString("componentName", value.ComponentName);
        writer.WriteString("version", value.Version);
        writer.WriteBoolean("isEnabled", value.IsEnabled);
        writer.WritePropertyName("schema");
        value.Schema.ToJsonDocument().WriteTo(writer);
        writer.WritePropertyName("mountingPoints");
        writer.WriteStartArray();
        foreach (var mountingPoint in value.MountingPoints)
        {
            writer.WriteStringValue(mountingPoint);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
