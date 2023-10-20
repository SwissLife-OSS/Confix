using System.Text.Json;

namespace Confix.Tool.Reporting;

public sealed record ReportingDependencyDefinition(
    IReadOnlyList<DependencyProviderDefinition> Providers)
{
    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(ReportingDependencyConfiguration.FieldNames.Providers);
        writer.WriteStartArray();
        foreach (var provider in Providers)
        {
            provider.WriteTo(writer);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public static ReportingDependencyDefinition From(ReportingDependencyConfiguration configuration)
    {
        var providers = configuration.Providers
            ?.Select(DependencyProviderDefinition.From)
            .ToArray() ?? Array.Empty<DependencyProviderDefinition>();

        return new ReportingDependencyDefinition(providers);
    }
}
