using System.Text.Json;
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
        var variables = GetVariables(node).ToArray();
        App.Log.DetectedVariables(variables.Length);

        var resolved = await _variableResolver.ResolveVariables(variables, cancellationToken);

        return new JsonVariableRewriter().Rewrite(node, new(resolved));
    }

    private static IEnumerable<VariablePath> GetVariables(JsonNode node)
        => JsonParser.ParseNode(node).Values
            .Where(v => v.GetSchemaValueType() == SchemaValueType.String)
            .SelectMany(v => v!.ToString().GetVariables());
}

file static class LogExtensions
{
    public static void DetectedVariables(this IConsoleLogger log, int count)
    {
        log.Information("Detected {0} variables", $"{count}".AsHighlighted());
    }
}