using System.Text.Json;
using Confix.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Reporting;

namespace Confix.Tool.Middlewares;

public sealed record ConfigurationFeature(
    ConfigurationScope Scope,
    IConfigurationFileCollection ConfigurationFiles,
    ProjectDefinition? Project,
    ComponentDefinition? Component,
    SolutionDefinition? Solution,
    EncryptionDefinition? Encryption,
    ReportingDefinition? Reporting)
{
    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("scope");
        writer.WriteStringValue(Scope.ToString());

        if (Project is not null)
        {
            writer.WritePropertyName(RuntimeConfiguration.FieldNames.Project);
            Project.WriteTo(writer);
        }

        if (Component is not null)
        {
            writer.WritePropertyName(RuntimeConfiguration.FieldNames.Component);
            Component.WriteTo(writer);
        }

        if (Encryption is not null)
        {
            writer.WritePropertyName(RuntimeConfiguration.FieldNames.Encryption);
            Encryption.WriteTo(writer);
        }

        if (Reporting is not null)
        {
            writer.WritePropertyName(RuntimeConfiguration.FieldNames.Reporting);
            Reporting.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}
