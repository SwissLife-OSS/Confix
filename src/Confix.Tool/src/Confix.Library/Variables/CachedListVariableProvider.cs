using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Nodes;
namespace Confix.Variables;

public class CachedListVariableProvider : IVariableProvider
{
    private readonly IVariableProvider _provider;
    private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _cache;

    public CachedListVariableProvider(
        IVariableProvider provider,
        ConcurrentDictionary<string, IReadOnlyList<string>> cache)
    {
        _provider = provider;
        _cache = cache;
    }

    public async Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(_provider);

        if (_cache.TryGetValue(cacheKey, out var cachedList))
        {
            return cachedList;
        }

        var result = await _provider.ListAsync(cancellationToken);

        _cache.TryAdd(cacheKey, result);

        return result;
    }

    public Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        return _provider.ResolveAsync(path, cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
    {
        return _provider.ResolveManyAsync(paths, cancellationToken);
    }

    public Task<string> SetAsync(string path, JsonNode value, CancellationToken ct)
    {
        return _provider.SetAsync(path, value, ct);
    }

    public ValueTask DisposeAsync()
    {
        return _provider.DisposeAsync();
    }

    private string GetCacheKey(IVariableProvider provider)
    {
        Type type = provider.GetType();
        PropertyInfo? typeProperty = type.GetProperty("Type");

        var value = (string?)typeProperty?.GetValue(null);

        if (value == null)
        {
            throw new VariableProviderCacheException(
                "Could not read the Type property of the provider.");
        }

        return value;
    }
}