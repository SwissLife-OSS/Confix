using System.Text.Json.Nodes;

namespace Confix.Variables;

public interface IVariableResolver
{
    Task<VariablePath> SetVariable(
        VariablePath path,
        JsonNode value,
        IVariableProviderContext context);

    Task<IEnumerable<VariablePath>> ListVariables(IVariableProviderContext context);

    Task<IEnumerable<VariablePath>> ListVariables(
        string providerName,
        IVariableProviderContext context);

    IEnumerable<string> ListProviders();

    string GetProviderType(string name);

    Task<JsonNode> ResolveVariable(VariablePath key, IVariableProviderContext context);

    Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        IVariableProviderContext context);
}
