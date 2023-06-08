using System.Text.Json;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public sealed class VariableReplacerService : IVariableReplacerService
{
    private readonly IVariableResolver _variableResolver;

    public VariableReplacerService(IVariableResolver variableResolver)
    {
        _variableResolver = variableResolver;
    }

    public async Task<JsonNode?> RewriteAsync(JsonNode? node, CancellationToken cancellationToken)
    {
        if (node is null)
        {
            return null;
        }
        var resolved = await _variableResolver.ResolveVariables(GetVariables(node).ToArray(), cancellationToken);

        return new JsonVariableRewriter(resolved).Rewrite(node);
    }

    private static IEnumerable<VariablePath> GetVariables(JsonNode node)
    {
        foreach (var value in JsonParser.ParseNode(node).Values)
        {
            if (value?.GetValue<JsonElement>().ValueKind == JsonValueKind.String)
            {
                if (VariablePath.TryParse(value.ToString(), out var parsed) && parsed.HasValue)
                {
                    yield return parsed.Value;
                }
            }
        }
    }
}
