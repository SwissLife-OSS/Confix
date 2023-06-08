using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public interface IVariableResolver
{
    Task<JsonNode> ResolveVariable(VariablePath key, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(IReadOnlyList<VariablePath> keys, CancellationToken cancellationToken);
}
