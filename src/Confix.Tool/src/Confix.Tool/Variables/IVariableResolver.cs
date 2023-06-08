using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public interface IVariableResolver
{
    Task<IEnumerable<VariablePath>> ListVariables(CancellationToken cancellationToken);
    Task<IEnumerable<VariablePath>> ListVariables(string providerName, CancellationToken cancellationToken);
    Task<JsonValue> ResolveVariable(VariablePath key, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<VariablePath, JsonValue>> ResolveVariables(IReadOnlyList<VariablePath> keys, CancellationToken cancellationToken);
}
