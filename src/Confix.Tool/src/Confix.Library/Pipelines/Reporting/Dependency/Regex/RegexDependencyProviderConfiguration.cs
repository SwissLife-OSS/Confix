using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Extensions;

namespace Confix.Tool.Reporting;

public sealed record RegexDependencyProviderConfiguration(string? Type, string? Kind, string? Regex)
{
    public static RegexDependencyProviderConfiguration Parse(JsonNode node)
        => node.Deserialize(JsonSerialization.Instance.RegexDependencyProviderConfiguration)!;
}
