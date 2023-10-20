using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Reporting;

public sealed record DependencyProviderConfiguration
{
    public static class FieldNames
    {
        public const string Type = "type";
    }

    public DependencyProviderConfiguration(string? type, JsonObject configuration)
    {
        Type = type;
        Configuration = configuration;
    }

    public string? Type { get; init; }

    public JsonObject Configuration { get; init; }

    public static DependencyProviderConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        return new DependencyProviderConfiguration(type, obj);
    }

    public DependencyProviderConfiguration Merge(DependencyProviderConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var type = other.Type ?? Type;
        var value = Configuration.Merge(other.Configuration)!.AsObject();

        return new DependencyProviderConfiguration(type, value);
    }
}
