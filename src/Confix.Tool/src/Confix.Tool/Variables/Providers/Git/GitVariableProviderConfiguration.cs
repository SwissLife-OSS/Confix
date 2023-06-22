using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record GitVariableProviderConfiguration(
    [property: JsonRequired]string RepositoryUrl,
    [property: JsonRequired]string FilePath,
    string[]? Arguments = null
)
{
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
