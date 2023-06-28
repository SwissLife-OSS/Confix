using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;
using Json.Schema;
using Spectre.Console;

namespace Confix.Tool.Middlewares;

public record ArtifactFileContext(DirectoryInfo ProjectRoot);

public sealed class ArtifactFileRewriter : JsonDocumentRewriter<ArtifactFileContext>
{
    protected override JsonNode Rewrite(JsonValue value, ArtifactFileContext context)
    {
        switch (value.GetSchemaValueType())
        {
            case SchemaValueType.String when (string?) value is { } strVal && IsPathValid(strVal):
                var rewritten = Path.GetRelativePath(context.ProjectRoot.FullName, strVal);
                App.Log.RewrittenPath(value, strVal, rewritten);
                return JsonValue.Create(rewritten);

            default:
                return base.Rewrite(value, context);
        }
    }

    private static bool IsPathValid(string path) => Path.IsPathRooted(path);

    public static ArtifactFileRewriter Instance { get; } = new();
}

file static class Log
{
    public static void RewrittenPath(
        this IConsoleLogger console,
        JsonValue path,
        string from,
        string to)
    {
        console.Information(
            $"Rewritten artifact file path in value {path.GetPath().EscapeMarkup()} from {from.EscapeMarkup().AsPath()} to {to.EscapeMarkup().AsPath()}");
    }
}
