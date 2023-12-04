using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;
using Json.Schema;

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
        CancellationToken cancellationToken)
    {
        var configuration = GetProviderConfiguration(path.ProviderName);

        await using var provider = _variableProviderFactory.CreateProvider(configuration);

        await provider.SetAsync(path.Path, value, cancellationToken);

        return path;
    }

    public async Task<IEnumerable<VariablePath>> ListVariables(CancellationToken cancellationToken)
    {
        var variables = await _configurations
            .Select(c => ListVariables(c.Name, cancellationToken))
            .ToListAsync(cancellationToken);

        return variables.SelectMany(v => v);
    }

    public async Task<IEnumerable<VariablePath>> ListVariables(
        string providerName,
        CancellationToken cancellationToken)
    {
        var configuration = GetProviderConfiguration(providerName);

        if (_variableListCache.TryGet(configuration, out var cachedList))
        {
            return cachedList;
        }

        await using var provider = _variableProviderFactory.CreateProvider(configuration);
        var variableKeys = await provider.ListAsync(cancellationToken);

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
        CancellationToken cancellationToken)
    {
        App.Log.ResolvingVariable(key);
        var configuration = GetProviderConfiguration(key.ProviderName);
        await using var provider = _variableProviderFactory.CreateProvider(configuration);
        
        return await provider.ResolveAsync(key.Path, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        CancellationToken cancellationToken)
    {
        List<KeyValuePair<VariablePath, JsonNode>> resolvedVariables = new(keys.Count);

        foreach (var group in keys.GroupBy(k => k.ProviderName))
        {
            var paths = group.Select(k => k.Path).Distinct().ToList();

            var providerResults = await ResolveVariables(group.Key, paths, cancellationToken);

            resolvedVariables.AddRange(providerResults);
        }

        return new Dictionary<VariablePath, JsonNode>(resolvedVariables);
    }

    private async Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        string providerName,
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
    {
        App.Log.ResolvingVariables(providerName, paths.Count);
        var resolvedVariables = new Dictionary<VariablePath, JsonNode>();
     
        var providerConfiguration = GetProviderConfiguration(providerName);
        await using var provider = _variableProviderFactory.CreateProvider(providerConfiguration);

        var resolvedValues = await provider.ResolveManyAsync(paths, cancellationToken);
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
