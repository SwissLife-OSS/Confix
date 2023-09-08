using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Extensions;

namespace Confix.Tool.Entities.Components.Git;

public sealed record GitComponentProviderConfiguration(
    string? Name,
    string? RepositoryUrl,
    string? Path,
    string[]? Arguments = null)
{
    public static GitComponentProviderConfiguration Parse(JsonNode node)
        => node.Deserialize(JsonSerialization.Instance.GitComponentProviderConfiguration)!;
}
