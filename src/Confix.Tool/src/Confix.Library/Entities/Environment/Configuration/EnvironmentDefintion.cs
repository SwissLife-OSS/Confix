using System.Text.Json;
using System.Text.Json.Serialization;

namespace Confix.Tool.Abstractions;

public sealed record EnvironmentDefinition(
    string Name,
    bool Enabled)
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
            configuration.Enabled ?? false);
    }

    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString(EnvironmentConfiguration.FieldNames.Name, Name);
        writer.WriteBoolean(EnvironmentConfiguration.FieldNames.Enabled, Enabled);
        writer.WriteEndObject();
    }

    public static EnvironmentDefinition Default { get; } = new("prod", true);
}
