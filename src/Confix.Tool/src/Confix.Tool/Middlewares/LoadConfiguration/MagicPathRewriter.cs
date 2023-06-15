using System.Text.Json.Nodes;
using Confix.Tool.Schema;
using Json.Schema;

namespace Confix.Tool.Middlewares;

public record MagicPathContext(
    DirectoryInfo HomeDirectory,
    DirectoryInfo? SolutionDirectory,
    DirectoryInfo? ProjectDirectory,
    DirectoryInfo FileDirectory
);

/// <summary>
///     Rewrites the magic paths in the configuration files.
/// </summary>
public sealed class MagicPathRewriter : JsonDocumentRewriter<MagicPathContext>
{
    protected override JsonNode Rewrite(JsonValue value, MagicPathContext context)
        => value.GetSchemaValueType() switch
        {
            SchemaValueType.String when value.ToMagicPath() is { } magicPath
                => value.RewriteMagicPath(magicPath, context),
            _ => base.Rewrite(value!, context)
        };
}

file enum MagicPath
{
    Home,
    Solution,
    Project,
    File
}

file static class MagicPathRewriterExtensions
{
    public static MagicPath? ToMagicPath(this JsonValue value)
    {
        if ((string?)value is { } stringValue)
        {
            return stringValue switch
            {
                string s when s.StartsWith("$home") => MagicPath.Home,
                string s when s.StartsWith("$solution") => MagicPath.Solution,
                string s when s.StartsWith("$project") => MagicPath.Project,
                string s when s.StartsWith("./") || s.StartsWith(".\\") => MagicPath.File,
                _ => null
            };
        }
        return null;
    }

    public static JsonNode RewriteMagicPath(
        this JsonValue value,
        MagicPath magicPath,
        MagicPathContext context)
    {
        var stringValue = ((string)value!).RemoveMagicPathPrefix(magicPath);
        return JsonValue.Create(
            magicPath switch
            {
                MagicPath.Home => Path.Combine(context.HomeDirectory.FullName, stringValue),

                MagicPath.Solution when context.SolutionDirectory is not null
                    => Path.Combine(context.SolutionDirectory.FullName, stringValue),

                MagicPath.Project when context.ProjectDirectory is not null
                    => Path.Combine(context.ProjectDirectory.FullName, stringValue),

                MagicPath.File => Path.Combine(context.FileDirectory.FullName, stringValue),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(magicPath),
                    magicPath,
                    "The magic path is not supported in this context")
            });
    }

    private static string RemoveMagicPathPrefix(this string value, MagicPath magicPath)
        => magicPath switch
        {
            MagicPath.Home => value.Remove(0, "$home".Length),
            MagicPath.Solution => value.Remove(0, "$solution".Length),
            MagicPath.Project => value.Remove(0, "$project".Length),
            _ => value
        };
}