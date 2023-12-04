using System.Collections.Immutable;
using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Json.Schema;

namespace Confix.Variables;

public sealed class VariableReplacerService : IVariableReplacerService
{
    private readonly IVariableResolver _variableResolver;

    public VariableReplacerService(IVariableResolver variableResolver)
    {
        _variableResolver = variableResolver;
    }


    public async Task<JsonNode?> RewriteAsync(JsonNode? node, CancellationToken cancellationToken)
    {
        if (node is null)
        {
            return null;
        }

        return await RewriteAsync(node, ImmutableHashSet<VariablePath>.Empty, cancellationToken);
    }


    private async Task<JsonNode> RewriteAsync(
        JsonNode node,
        IReadOnlySet<VariablePath> resolvedPaths,
        CancellationToken cancellationToken)
    {
        var resolvedVariables = await ResolveVariables(node, resolvedPaths, cancellationToken);

        return new JsonVariableRewriter().Rewrite(node, new(resolvedVariables));
    }

    private static IEnumerable<VariablePath> GetVariables(JsonNode node)
    {
        if (node is JsonValue value)
        {
            return value.GetSchemaValueType() == SchemaValueType.String
                ? value.ToString().GetVariables()
                : Enumerable.Empty<VariablePath>();
        }

        return JsonParser.ParseNode(node).Values
                    .Where(v => v.GetSchemaValueType() == SchemaValueType.String)
                    .SelectMany(v => v!.ToString().GetVariables());
    }

    private async Task<IReadOnlyDictionary<VariablePath, JsonNode>> ResolveVariables(
        JsonNode node,
        IReadOnlySet<VariablePath> resolvedPaths,
        CancellationToken cancellationToken)
    {
        var variables = GetVariables(node).ToArray();
        if (variables.Length == 0)
        {
            return ImmutableDictionary<VariablePath, JsonNode>.Empty;
        }
        if (resolvedPaths.Overlaps(variables))
        {
            throw new CircularVariableReferenceException(variables.First(v => resolvedPaths.Contains(v)));
        }

        var resolvedVariables = await _variableResolver.ResolveVariables(variables, cancellationToken);

        var resolved = new Dictionary<VariablePath, JsonNode>();
        foreach (var variable in resolvedVariables)
        {
            var currentPath = new HashSet<VariablePath>() { variable.Key };
            currentPath.UnionWith(resolvedPaths);
            resolved[variable.Key] = await RewriteAsync(
                variable.Value,
                currentPath,
                cancellationToken);
        }

        return resolved;
    }
}

file static class LogExtensions
{
    public static void DetectedVariables(this IConsoleLogger log, int count)
    {
        log.Information("Detected {0} variables", $"{count}".AsHighlighted());
    }
}