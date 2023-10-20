using System.Text.Json.Nodes;
using Confix.Utilities.Json;
using Json.Schema;

namespace Confix.Tool.Reporting;

public sealed record ReportingDependencyConfiguration
{
    public static class FieldNames
    {
        public const string Providers = "providers";
    }

    public ReportingDependencyConfiguration(
        IReadOnlyList<DependencyProviderConfiguration>? providers)
    {
        Providers = providers;
    }

    public IReadOnlyList<DependencyProviderConfiguration>? Providers { get; }

    public static ReportingDependencyConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var providers = obj
            .MaybeProperty(FieldNames.Providers)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(DependencyProviderConfiguration.Parse)
            .ToArray();

        return new ReportingDependencyConfiguration(providers);
    }

    public ReportingDependencyConfiguration Merge(ReportingDependencyConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var providers = (Providers, other.Providers)
            .MergeWith(
                (x, y) => x.Type == y.Type && x.GetKind() == y.GetKind(),
                (x, y) => x.Merge(y));

        return new ReportingDependencyConfiguration(providers);
    }
}

file static class Extensions
{
    public static string? GetKind(this DependencyProviderConfiguration? configuration)
    {
        if (configuration is null)
        {
            return null;
        }

        if (!configuration.Configuration.TryGetPropertyValue("Kind", out var kind) ||
            kind is not JsonValue value ||
            value.GetSchemaValueType() is not SchemaValueType.String)
        {
            return null;
        }

        return value.GetValue<string>();
    }
}
