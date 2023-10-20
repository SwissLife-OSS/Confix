using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Confix.Entities.Schema;
using Confix.Tool.Schema;
using Json.More;
using Json.Pointer;
using Json.Schema;

namespace Confix.Tool.Reporting;

public sealed class GraphQLDependencyProvider : IDependencyProvider
{
    private const string _contextDataKey = "Confix.Tool.Reporting.EvaluationResults";

    public static string Type => "graphql";

    public void Analyze(DependencyAnalyzerContext context, JsonNode node)
    {
        if (!context.TryGetContextData(_contextDataKey, out EvaluationResults? results))
        {
            var document = context.GetSchema(Type);
            var options = new EvaluationOptions()
            {
                OutputFormat = Json.Schema.OutputFormat.List,
                PreserveDroppedAnnotations = true,
                ProcessCustomKeywords = true,
            };
            results = document.Evaluate(context.Document, options);
            context.SetContextData(_contextDataKey, results);
        }

        var pointerAsString = node.GetPointerFromRoot();
        var pointer = JsonPointer.Parse(pointerAsString);

        var kinds = results?.Details
                .Where(x => x.InstanceLocation == pointer &&
                    x.Annotations?.ContainsKey(MetadataKeyword.Name) is true)
                .Select(x => x.Annotations![MetadataKeyword.Name])
                .OfType<JsonArray>()
                .SelectMany(x => x)
                .OfType<JsonObject>()
                .Where(IsDependency)
                .Select(x => x["kind"]!.ToString())
            ?? Enumerable.Empty<string>();

        foreach (var kind in kinds)
        {
            var dependency = new GraphQLDependency(Type, kind, node.GetPointerFromRoot(), node);
            context.AddDependency(dependency);
        }
    }

    private static bool IsDependency(JsonNode obj)
    {
        return obj.TryGetPropertyValue("type", out string? type) &&
            type == "dependency" &&
            obj.TryGetPropertyValue("kind", out string? kind) &&
            !string.IsNullOrEmpty(kind);
    }
}

file static class Extensions
{
    public static bool TryGetPropertyValue<T>(
        this JsonNode node,
        string propertyName,
        out T? value)
    {
        value = default;
        if (node is not JsonObject obj)
        {
            return false;
        }

        if (!obj.TryGetPropertyValue(propertyName, out var propertyValueNode) ||
            propertyValueNode is not JsonValue valueNode)
        {
            return false;
        }

        return valueNode.TryGetValue(out value);
    }
}
