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
        => JsonParser.ParseNode(node)
            .Values
            .Where(v => v is not null)
            .Select(v => { _ = VariablePath.TryParse(v!, out var parsed); return parsed; })
            .Where(p => p.HasValue)
            .Select(p => p!.Value);
}
