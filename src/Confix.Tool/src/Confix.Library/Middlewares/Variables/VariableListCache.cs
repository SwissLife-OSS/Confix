using System.Collections.Concurrent;
using Confix.Variables;
namespace Confix.Tool.Middlewares;

public class VariableListCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<VariablePath>> _cache = new();
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
            _cache.TryGetValue(GetCacheKey(configuration), out var cachedValues))
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
            return _cache.TryAdd(GetCacheKey(configuration), variables);
        }

        return false;
    }
    
    private bool CacheEnabled(VariableProviderConfiguration configuration) =>
        _cacheEnabledVariableProviderTypes.Contains(
            configuration.Type,
            StringComparer.CurrentCultureIgnoreCase);
    
    private string GetCacheKey(VariableProviderConfiguration configuration) =>
        configuration.Configuration.ToString();
}