using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record GitVariableProviderConfiguration
{
    public required string RepositoryUrl { get; init; }

    public required string FilePath { get; init; }

    public string[]? Arguments { get; init; }

    public static GitVariableProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(JsonSerialization.Instance.GitVariableProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Configuration of GitVariableProvider is invalid",
                ex);
        }
    }
}
