using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;

namespace Confix.Variables;

public sealed class VariableResolver : IVariableResolver
{
    private readonly IVariableProviderFactory _variableProviderFactory;
    private readonly VariableListCache _variableListCache;
    private readonly IReadOnlyList<VariableProviderConfiguration> _configurations;

    public VariableResolver(
        IVariableProviderFactory variableProviderFactory,
        VariableListCache variableListCache,
        IReadOnlyList<VariableProviderConfiguration> configurations)
    {
        _variableProviderFactory = variableProviderFactory;
        _variableListCache = variableListCache;
        _configurations = configurations;
    }

    public async Task<VariablePath> SetVariable(
        VariablePath path,
        JsonNode value,
        IVariableProviderContext context)
    {
        var configuration = GetProviderConfiguration(path.ProviderName);

        await using var provider = _variableProviderFactory.CreateProvider(configuration);

        await provider.SetAsync(path.Path, value, context);

        return path;
    }

    public async Task<IEnumerable<VariablePath>> ListVariables(IVariableProviderContext context)
    {
        var variables = await _configurations
            .Select(c => ListVariables(c.Name, context))
            .ToListAsync(context.CancellationToken);

        return variables.SelectMany(v => v);
    }

    public async Task<IEnumerable<VariablePath>> ListVariables(
        string providerName,
        IVariableProviderContext context)
    {
        var configuration = GetProviderConfiguration(providerName);

        if (_variableListCache.TryGet(configuration, out var cachedList))
        {
            return cachedList;
        }

        await using var provider = _variableProviderFactory.CreateProvider(configuration);
        var variableKeys = await provider.ListAsync(context);

        var items = variableKeys.Select(k => new VariablePath(providerName, k)).ToArray();
        _variableListCache.TryAdd(configuration, items);

        return items;
    }

    /// <inheritdoc />
    public IEnumerable<string> ListProviders()
        => _configurations.Select(c => c.Name);

    /// <inheritdoc />
    public string GetProviderType(string name)
    {
        var configuration = GetProviderConfiguration(name);
        return configuration.Type;
    }

    public async Task<JsonNode> ResolveVariable(
        VariablePath key,
        IVariableProviderContext context)
    {
        App.Log.ResolvingVariable(key);
        var configuration = GetProviderConfiguration(key.ProviderName);
        await using var provider = _variableProviderFactory.CreateProvider(configuration);
        
        return await provider.ResolveAsync(key.Path, context);
    }

    public async Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        IVariableProviderContext context)
    {
        List<KeyValuePair<VariablePath, JsonNode>> resolvedVariables = new(keys.Count);

        foreach (var group in keys.GroupBy(k => k.ProviderName))
        {
            var paths = group.Select(k => k.Path).Distinct().ToList();

            var providerResults = await ResolveVariables(group.Key, paths, context);

            resolvedVariables.AddRange(providerResults);
        }

        return new Dictionary<VariablePath, JsonNode>(resolvedVariables);
    }

    private async Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        string providerName,
        IReadOnlyList<string> paths,
        IVariableProviderContext context)
    {
        App.Log.ResolvingVariables(providerName, paths.Count);
        var resolvedVariables = new Dictionary<VariablePath, JsonNode>();
     
        var providerConfiguration = GetProviderConfiguration(providerName);
        await using var provider = _variableProviderFactory.CreateProvider(providerConfiguration);

        var resolvedValues = await provider.ResolveManyAsync(paths, context);
        foreach (var (key, value) in resolvedValues)
        {
            resolvedVariables.Add(new(providerName, key), value);
        }

        return resolvedVariables;
    }

    private VariableProviderConfiguration GetProviderConfiguration(string providerName)
        => _configurations.FirstOrDefault(c => c.Name.Equals(providerName))
            ?? throw new ExitException(
                $"No VariableProvider with name '{providerName.AsHighlighted()}' configured.");
}

file static class Log
{
    public static void ResolvingVariable(this IConsoleLogger log, VariablePath key)
    {
        log.Information("Resolving variable {0}", $"{key}".AsHighlighted());
    }

    public static void ResolvingVariables(this IConsoleLogger log, string providerName, int count)
    {
        log.Information(
            "Resolving {0} variables from {1}",
            $"{count}".AsHighlighted(),
            providerName.AsHighlighted());
    }
}
