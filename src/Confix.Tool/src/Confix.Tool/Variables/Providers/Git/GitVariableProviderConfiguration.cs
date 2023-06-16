using System.IO;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ConfiX.Extensions;

namespace ConfiX.Variables;

public sealed record GitVariableProviderConfiguration
{
    [JsonPropertyName("repositoryUrl")]
    public required string RepositoryUrl { get; init; }

    [DefaultValue("main")]
    [JsonPropertyName("branch")]
    public required string Branch { get; init; }

    [JsonPropertyName("filePath")]
    public required string FilePath { get; init; }

    [JsonPropertyName("cloneDirectory")]
    // todo: default of .confix/.variables/git/hash would be cool
    public required string CloneDirectory { get; init; } 

    [DefaultValue(1)]
    [JsonPropertyName("depth")]
    public required int Depth { get; init; }

    [JsonPropertyName("arguments")]
    public string? Arguments { get; init; }


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
