using Confix.Tool;

namespace ConfiX.Variables;

public interface IVariableResolver
{
    Task<IReadOnlyDictionary<VariablePath, string>> ResolveVariables(IReadOnlyList<VariablePath> keys, CancellationToken cancellationToken);
}

public sealed class VariableResolver : IVariableResolver
{
    private readonly IVariableProviderFactory _variableProviderFactory;

    public VariableResolver(IVariableProviderFactory variableProviderFactory)
    {
        _variableProviderFactory = variableProviderFactory;
    }

    public async Task<IReadOnlyDictionary<VariablePath, string>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        CancellationToken cancellationToken)
    {
        Dictionary<VariablePath, string> resolvedVariables = new();

        foreach (IGrouping<string, VariablePath> group in keys.GroupBy(k => k.ProviderName))
        {
            IVariableProvider provider = _variableProviderFactory.CreateProvider(group.Key);

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
}
