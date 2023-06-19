using System.Text.Json.Nodes;
using Confix.Tool;

namespace ConfiX.Variables;

public sealed class VariableResolver : IVariableResolver
{
    private readonly IVariableProviderFactory _variableProviderFactory;
    private readonly IReadOnlyList<VariableProviderConfiguration> _configurations;

    public VariableResolver(
        IVariableProviderFactory variableProviderFactory,
        IReadOnlyList<VariableProviderConfiguration> configurations)
    {
        _variableProviderFactory = variableProviderFactory;
        _configurations = configurations;
    }

    public async Task<VariablePath> SetVariable(
        string providerName,
        string key,
        JsonNode value,
        CancellationToken cancellationToken)
    {
        var configuration = GetProviderConfiguration(providerName);
        await using var provider = _variableProviderFactory.CreateProvider(configuration);
        var providerPath = await provider.SetAsync(key, value, cancellationToken);

        return new VariablePath(providerName, providerPath);
    }

    public Task<IEnumerable<VariablePath>> ListVariables(CancellationToken cancellationToken)
    {
        var tasks = _configurations.Select(c => ListVariables(c.Name, cancellationToken));
        return Task.WhenAll(tasks).ContinueWith(t => t.Result.SelectMany(x => x));
    }

    public async Task<IEnumerable<VariablePath>> ListVariables(string providerName, CancellationToken cancellationToken)
    {
        var configuration = GetProviderConfiguration(providerName);
        await using var provider = _variableProviderFactory.CreateProvider(configuration);
        var variableKey = await provider.ListAsync(cancellationToken);

        return variableKey.Select(k => new VariablePath(providerName, k));
    }

    public async Task<JsonNode> ResolveVariable(VariablePath key, CancellationToken cancellationToken)
    {
        var configuration = GetProviderConfiguration(key.ProviderName);
        await using var provider = _variableProviderFactory.CreateProvider(configuration);
        return await provider.ResolveAsync(key.Path, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        CancellationToken cancellationToken)
    {
        Dictionary<VariablePath, JsonNode> resolvedVariables = new();

        foreach (IGrouping<string, VariablePath> group in keys.GroupBy(k => k.ProviderName))
        {
            var providerConfiguration = GetProviderConfiguration(group.Key);
            await using IVariableProvider provider = _variableProviderFactory.CreateProvider(providerConfiguration);

            var resolved = await provider.ResolveManyAsync(
                group.Select(x => x.Path).ToArray(),
                cancellationToken);

            resolved.ForEach((r) =>
                resolvedVariables.Add(
                    group.First(i => i.Path == r.Key),
                    r.Value));
        }

        return resolvedVariables;
    }

    private VariableProviderConfiguration GetProviderConfiguration(string providerName)
      => _configurations.FirstOrDefault(c => c.Name.Equals(providerName))
          ?? throw new InvalidOperationException($"Provider '{providerName}' not found");
}
