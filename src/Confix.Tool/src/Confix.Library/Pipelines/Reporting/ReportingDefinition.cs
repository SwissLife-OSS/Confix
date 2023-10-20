using System.Text.Json;

namespace Confix.Tool.Reporting;

public sealed record ReportingDefinition(ReportingDependencyDefinition? Dependencies)
{
    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        if (Dependencies is not null)
        {
            writer.WritePropertyName(ReportingConfiguration.FieldNames.Dependencies);
            Dependencies.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    public static ReportingDefinition From(ReportingConfiguration configuration)
    {
        var dependencies = configuration.Dependencies is not null
            ? ReportingDependencyDefinition.From(configuration.Dependencies)
            : null;

        return new ReportingDefinition(dependencies);
    }
}
