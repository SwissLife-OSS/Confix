using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Commands.Logging;
using Json.Schema;

namespace Confix.Variables;

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
        App.Log.ResolvingVariable(key);
        var configuration = GetProviderConfiguration(key.ProviderName);
        await using var provider = _variableProviderFactory.CreateProvider(configuration);
        var resolvedValue = await provider.ResolveAsync(key.Path, cancellationToken);

        if (resolvedValue.IsVariable(out VariablePath? variablePath))
        {
            return await ResolveVariable(variablePath.Value, cancellationToken);
        }
        return resolvedValue;
    }

    public async Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        IReadOnlyList<VariablePath> keys,
        CancellationToken cancellationToken)
    {
        List<KeyValuePair<VariablePath, JsonNode>> resolvedVariables = new(keys.Count);

        foreach (IGrouping<string, VariablePath> group in keys.GroupBy(k => k.ProviderName))
        {
            var providerResults = await ResolveVariables(
                group.Key,
                group.Select(k => k.Path).Distinct().ToList(),
                cancellationToken);

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
        Dictionary<VariablePath, JsonNode> resolvedVariables = new();
        Dictionary<VariablePath, string> nestedVariables = new();

        var providerConfiguration = GetProviderConfiguration(providerName);
        await using IVariableProvider provider = _variableProviderFactory.CreateProvider(providerConfiguration);

        var resolved = await provider.ResolveManyAsync(paths, cancellationToken);
        foreach (KeyValuePair<string, JsonNode> r in resolved)
        {
            if (r.Value.IsVariable(out VariablePath? variablePath))
            {
                nestedVariables.Add(variablePath.Value, r.Key);
            }
            else
            {
                resolvedVariables.Add(
                    new(providerName, r.Key),
                    r.Value);
            }
        }

        var resolvedNestedVariables = await ResolveVariables(nestedVariables.Keys.ToList(), cancellationToken);
        foreach (KeyValuePair<VariablePath, JsonNode> r in resolvedNestedVariables)
        {
            resolvedVariables.Add(
                new(providerName, nestedVariables[r.Key]),
                r.Value);
        }

        return resolvedVariables;
    }

    private VariableProviderConfiguration GetProviderConfiguration(string providerName)
      => _configurations.FirstOrDefault(c => c.Name.Equals(providerName))
          ?? throw new InvalidOperationException($"Provider '{providerName}' not found");
}

public static class Extension
{
    public static bool IsVariable(
        this JsonNode node,
        [NotNullWhen(true)] out VariablePath? variablePath)
    {
        if (node.GetSchemaValueType() == SchemaValueType.String
            && VariablePath.TryParse((string)node!, out VariablePath? parsed))
        {
            variablePath = parsed;
            return true;
        }

        variablePath = default;
        return false;
    }
}

file static class LogExtensionts
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