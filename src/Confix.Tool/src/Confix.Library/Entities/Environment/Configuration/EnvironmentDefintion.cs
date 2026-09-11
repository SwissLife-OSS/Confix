using System.Text.Json;
using System.Text.Json.Serialization;

namespace Confix.Tool.Abstractions;

public sealed record EnvironmentDefinition(
    string Name,
    bool Enabled,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ValidationConfiguration? Validation = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] bool? ExportSchema = null)
{
    public static EnvironmentDefinition From(EnvironmentConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            throw new ValidationException("EnvironmentDefinition is invalid")
            {
                Errors = new[] { "Name is null or empty" }
            };
        }

        return new EnvironmentDefinition(
            configuration.Name,
            configuration.Enabled ?? false, configuration.Validation, configuration.ExportSchema);
    }

    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString(EnvironmentConfiguration.FieldNames.Name, Name);
        writer.WriteBoolean(EnvironmentConfiguration.FieldNames.Enabled, Enabled);
        if (ExportSchema is { } exportSchema)
        {
            writer.WriteBoolean(EnvironmentConfiguration.FieldNames.ExportSchema, exportSchema);
        }
        if (Validation is not null)
        {
            writer.WritePropertyName(EnvironmentConfiguration.FieldNames.Validation);
            JsonSerializer.Serialize(writer, Validation, ValidationConfiguration.SerializerOptions);
        }
        writer.WriteEndObject();
    }

    public static EnvironmentDefinition Default { get; } = new("prod", true);
}
