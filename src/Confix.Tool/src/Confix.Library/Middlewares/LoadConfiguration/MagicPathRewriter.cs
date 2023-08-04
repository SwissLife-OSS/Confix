using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;
using Json.Schema;

namespace Confix.Tool.Middlewares;

public record MagicPathContext(
    DirectoryInfo HomeDirectory,
    DirectoryInfo? SolutionDirectory,
    DirectoryInfo? ProjectDirectory,
    DirectoryInfo FileDirectory);

/// <summary>
///     Rewrites the magic paths in the configuration files.
/// </summary>
public sealed class MagicPathRewriter : JsonDocumentRewriter<MagicPathContext>
{
    protected override JsonNode Rewrite(JsonValue value, MagicPathContext context)
    {
        switch (value.GetSchemaValueType())
        {
            case SchemaValueType.String when MagicPath.From(value) is { } magicPath:
                var replacedValue = magicPath.Replace(context);

                App.Log.ReplacedMagicPath((string?) value, replacedValue);

                return JsonValue.Create(replacedValue)!;

            default:
                return base.Rewrite(value!, context);
        }
    }
}

file enum Scope
{
    Home,
    Solution,
    Project,
    File
}

file class MagicPath
{
    private static class Prefix
    {
        public const string Home = "$home:/";
        public const string Tilde = "~/";
        public const string Solution = "$solution:/";
        public const string Project = "$project:/";
        public const string FileLinux = "./";
        public const string FileWindows = ".\\";
    }

    private readonly Scope _type;
    private readonly string _path;
    private readonly string _original;

    public MagicPath(Scope type, string path, string original)
    {
        _type = type;
        _path = path;
        _original = original;
    }

    public static MagicPath? From(JsonValue jsonValue)
    {
        if ((string?) jsonValue is not { } value)
        {
            return null;
        }

        return value switch
        {
            not null when value.StartsWith(Prefix.Home)
                => new MagicPath(Scope.Home, value.RemovePrefix(Prefix.Home), value),

            not null when value.StartsWith(Prefix.Tilde)
                => new MagicPath(Scope.Home, value.RemovePrefix(Prefix.Tilde), value),

            not null when value.StartsWith(Prefix.Solution)
                => new MagicPath(Scope.Solution, value.RemovePrefix(Prefix.Solution), value),

            not null when value.StartsWith(Prefix.Project)
                => new MagicPath(Scope.Project, value.RemovePrefix(Prefix.Project), value),

            not null when value.StartsWith(Prefix.FileLinux)
                => new MagicPath(Scope.File, value.RemovePrefix(Prefix.FileLinux), value),

            not null when value.StartsWith(Prefix.FileWindows)
                => new MagicPath(Scope.File, value.RemovePrefix(Prefix.FileWindows), value),
            _ => null
        };
    }

    public string Replace(MagicPathContext context)
    {
        switch (_type)
        {
            case Scope.Home:
                return Path.Combine(context.HomeDirectory.FullName, _path);

            case Scope.Solution when context.SolutionDirectory is not null:
                return Path.Combine(context.SolutionDirectory.FullName, _path);

            case Scope.Project when context.ProjectDirectory is not null:
                return Path.Combine(context.ProjectDirectory.FullName, _path);

            case Scope.File:
                return Path.Combine(context.FileDirectory.FullName, _path);

            default:
                App.Log.NotSupportedInContext(_type);
                return _original;
        }
    }
}

file static class Extensions
{
    public static string RemovePrefix(this string value, string prefix)
        => value.Remove(0, prefix.Length);
}

file static class LogExtensions
{
    public static void NotSupportedInContext(this IConsoleLogger log, Scope type)
        => log.Debug("Magic path type {0} is not supported in this context.", type.ToString());

    public static void ReplacedMagicPath(
        this IConsoleLogger log,
        string originalValue,
        string replacedValue)
        => log.Debug("Replaced magic path {0} with {1}.", originalValue, replacedValue);
}
