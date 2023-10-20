using System.Text.Json.Nodes;
using Confix.Extensions;
using Confix.Utilities.Json;

namespace Confix.Tool.Reporting;

public sealed record ReportingConfiguration
{
    public static class FieldNames
    {
        public const string Dependencies = "dependencies";
    }

    public ReportingConfiguration(ReportingDependencyConfiguration? dependencies)
    {
        Dependencies = dependencies;
    }

    public ReportingDependencyConfiguration? Dependencies { get; init; }

    public static ReportingConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var dependencies = obj.TryGetNonNullPropertyValue(FieldNames.Dependencies, out var deps)
            ? ReportingDependencyConfiguration.Parse(deps.ExpectObject())
            : null;

        return new ReportingConfiguration(dependencies);
    }

    public ReportingConfiguration Merge(ReportingConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var dependencies = Dependencies?.Merge(other.Dependencies) ?? other.Dependencies;

        return new ReportingConfiguration(dependencies);
    }
}