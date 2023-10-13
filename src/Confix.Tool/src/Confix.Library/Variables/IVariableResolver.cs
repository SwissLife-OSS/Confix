using System.Text.Json.Nodes;

namespace Confix.Variables;

public interface IVariableResolver
{
    Task<VariablePath> SetVariable(
        VariablePath path,
        JsonNode value,
        CancellationToken cancellationToken);

    Task<IEnumerable<VariablePath>> ListVariables(CancellationToken cancellationToken);

    Task<IEnumerable<VariablePath>> ListVariables(
        string providerName,
        CancellationToken cancellationToken);

    IEnumerable<string> ListProviders();

    string GetProviderType(string name);

    Task<JsonNode> ResolveVariable(VariablePath key, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        CancellationToken cancellationToken);
}
