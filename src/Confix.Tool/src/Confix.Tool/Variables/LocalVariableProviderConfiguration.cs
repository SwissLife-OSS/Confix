using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public record LocalVariableProviderConfiguration
{
    public required string FilePath { get; init; }
    public static LocalVariableProviderConfiguration Parse(JsonNode node)
    {
        var parsed = JsonParser.ParseNode(node);

        return new LocalVariableProviderConfiguration
        {
            FilePath = parsed.GetValueOrDefault("path") ?? throw new ArgumentException("""Configuration of LocalVariableProvider is missing the required property "path" specifying the path where the variable file is located""")
        };
    }
}
