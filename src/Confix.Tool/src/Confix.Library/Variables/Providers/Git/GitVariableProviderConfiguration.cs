using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Extensions;

namespace Confix.Variables;

public sealed record GitVariableProviderConfiguration(
    string? RepositoryUrl,
    string? FilePath,
    string[]? Arguments = null
)
{
    public static GitVariableProviderConfiguration Parse(JsonNode node)
        => node.Deserialize(JsonSerialization.Instance.GitVariableProviderConfiguration)!;
}
