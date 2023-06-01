using Confix.Tool;

namespace ConfiX.Variables;

public class VariableResolver
{
    private readonly IVariableProviderFactory _variableProviderFactory;

    public VariableResolver(IVariableProviderFactory variableProviderFactory)
    {
        _variableProviderFactory = variableProviderFactory;
    }

    public async Task<IReadOnlyDictionary<VariablePath, string>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        IReadOnlyList<VariableProviderConfiguration> configurations,
        CancellationToken cancellationToken)
    {
        Dictionary<VariablePath, string> resolvedVariables = new();

        foreach (IGrouping<string, VariablePath> group in keys.GroupBy(k => k.ProviderName))
        {
            VariableProviderConfiguration configuration = GetProviderConfiguration(configurations, group.Key);
            IVariableProvider provider = _variableProviderFactory.CreateProvider(
                configuration.Type,
                configuration.Configuration);

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

    private static VariableProviderConfiguration GetProviderConfiguration(
        IReadOnlyList<VariableProviderConfiguration> configurations,
        string providerName) 
        => configurations.FirstOrDefault(c => c.Name.Equals(providerName)) 
            ?? throw new InvalidOperationException("Provider not found");


}
