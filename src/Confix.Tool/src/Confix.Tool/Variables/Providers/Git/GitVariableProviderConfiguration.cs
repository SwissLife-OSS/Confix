using System.Text.Json;
using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record GitVariableProviderConfiguration(
    string RepositoryUrl,
    string FilePath,
    string[]? Arguments
)
{
    public static GitVariableProviderConfiguration Parse(JsonNode node)
    {
        try
        {
            return node.Deserialize(JsonSerialization.Default.GitVariableProviderConfiguration)!;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "Configuration of GitVariableProvider is invalid",
                ex);
        }
    }
}
