using Confix.Tool;

namespace ConfiX.Variables;

public interface IVariableResolver
{
    Task<IReadOnlyDictionary<VariablePath, string>> ResolveVariables(IReadOnlyList<VariablePath> keys, CancellationToken cancellationToken);
}

public sealed class VariableResolver : IVariableResolver
{
    private readonly IVariableProviderFactory _variableProviderFactory;
    private readonly IReadOnlyList<VariableProviderConfiguration> _configurations;

    public VariableResolver(IVariableProviderFactory variableProviderFactory, IReadOnlyList<VariableProviderConfiguration> configurations)
    {
        _variableProviderFactory = variableProviderFactory;
        _configurations = configurations;
    }

    public async Task<IReadOnlyDictionary<VariablePath, string>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        CancellationToken cancellationToken)
    {
        Dictionary<VariablePath, string> resolvedVariables = new();

        foreach (IGrouping<string, VariablePath> group in keys.GroupBy(k => k.ProviderName))
        {

            var providerConfiguration = GetProviderConfiguration(group.Key);
            IVariableProvider provider = _variableProviderFactory.CreateProvider(providerConfiguration);

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
