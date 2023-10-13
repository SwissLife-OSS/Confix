using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Json.More;
using Json.Schema;

namespace Confix.Variables;

public sealed class VariableExtractorService
    : IVariableExtractorService
{
    private readonly IVariableResolver _variableResolver;

    public VariableExtractorService(IVariableResolver variableResolver)
    {
        _variableResolver = variableResolver;
    }

    public async Task<IEnumerable<VariableInfo>> ExtractAsync(
        JsonNode? node,
        CancellationToken cancellationToken)
    {
        if (node is null)
        {
            return Array.Empty<VariableInfo>();
        }

        var variables = Extract(node).ToArray();

        App.Log.DetectedVariables(variables.Length);

        var resolved = await _variableResolver
            .ResolveVariables(variables.Select(x => x.Path).ToArray(), cancellationToken);

        var infos = new List<VariableInfo>();

        foreach (var variable in variables)
        {
            var resolvedValue = resolved[variable.Path];
            var providerName = variable.Path.ProviderName;
            var providerType = _variableResolver.GetProviderType(providerName);

            var info = new VariableInfo(
                providerName,
                providerType,
                variable.Path.Path,
                resolvedValue.GetSchemaValueType() switch
                {
                    SchemaValueType.String => resolvedValue.GetValue<string>(),
                    SchemaValueType.Null => "null",
                    _ => resolvedValue.ToJsonString()
                },
                variable.Node.GetPathFromRoot());

            infos.Add(info);
        }

        return infos;
    }

    private static IEnumerable<ExtractedVariable> Extract(JsonNode node)
        => JsonParser.ParseNode(node)
            .Values
            .Where(v => v.GetSchemaValueType() == SchemaValueType.String)
            .SelectMany(v =>
            {
                var extractedVariables = new List<ExtractedVariable>();
                foreach (var variablePath in v!.ToString().GetVariables())
                {
                    extractedVariables.Add(new(v, variablePath));
                }

                return extractedVariables;
            });

    private sealed record ExtractedVariable(JsonNode Node, VariablePath Path);
}

file static class LogExtensions
{
    public static void DetectedVariables(this IConsoleLogger log, int count)
    {
        log.Information("Detected {0} variables", $"{count}".AsHighlighted());
    }
}
