using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public interface IVariableResolver
{
    Task<JsonValue> ResolveVariable(VariablePath key, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<VariablePath, JsonValue>> ResolveVariables(IReadOnlyList<VariablePath> keys, CancellationToken cancellationToken);
}
