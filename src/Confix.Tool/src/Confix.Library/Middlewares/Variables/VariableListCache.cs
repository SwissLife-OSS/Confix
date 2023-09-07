using System.Collections.Concurrent;
using Confix.Variables;
namespace Confix.Tool.Middlewares;

public class VariableListCache
{
    private readonly ConcurrentDictionary<(string, string), IReadOnlyList<VariablePath>> _cache = new();
    private readonly IReadOnlyList<string> _cacheEnabledVariableProviderTypes = new[]
    {
        GitVariableProvider.Type,
        AzureKeyVaultProvider.Type
    };

    public bool TryGet(
        VariableProviderConfiguration configuration,
        out IReadOnlyList<VariablePath> variables)
    {
        if (CacheEnabled(configuration) &&
            _cache.TryGetValue((configuration.Name, configuration.Type), out var cachedValues))
        {
            variables = cachedValues;
            return true;
        }

        variables = Array.Empty<VariablePath>();

        return false;
    }

    public bool TryAdd(
        VariableProviderConfiguration configuration,
        IReadOnlyList<VariablePath> variables)
    {
        if (CacheEnabled(configuration))
        {
            return _cache.TryAdd((configuration.Name, configuration.Type), variables);
        }

        return false;
    }
        

    private bool CacheEnabled(VariableProviderConfiguration configuration) =>
        _cacheEnabledVariableProviderTypes.Contains(
            configuration.Type,
            StringComparer.CurrentCultureIgnoreCase);
}