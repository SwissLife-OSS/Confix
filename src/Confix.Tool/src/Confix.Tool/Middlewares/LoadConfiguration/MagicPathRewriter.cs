using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
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
    {
        switch (value.GetSchemaValueType())
        {
            case SchemaValueType.String when MagicPath.From(value) is { } magicPath:
                string replacedValue = magicPath.Replace(context);
                App.Log.ReplacedMagicPath(magicPath, replacedValue);
                return JsonValue.Create(replacedValue);
            default:
                return base.Rewrite(value!, context);
        }
    }
}

file enum MagicPathType
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

    private readonly MagicPathType _type;
    private readonly string _path;
    private readonly string _original;

    public MagicPath(MagicPathType type, string path, string original)
    {
        _type = type;
        _path = path;
        _original = original;
    }

    public static MagicPath? From(JsonValue value)
    {
        if ((string?)value is { } stringValue)
        {
            return stringValue switch
            {
                string s when s.StartsWith(Prefix.Home)
                    => new MagicPath(
                        MagicPathType.Home,
                        s.Remove(0, Prefix.Home.Length),
                        stringValue),
                string s when s.StartsWith(Prefix.Tilde)
                    => new MagicPath(
                        MagicPathType.Home,
                        s.Remove(0, Prefix.Tilde.Length),
                        stringValue),
                string s when s.StartsWith(Prefix.Solution)
                    => new MagicPath(
                        MagicPathType.Solution,
                        s.Remove(0, Prefix.Solution.Length),
                        stringValue),
                string s when s.StartsWith(Prefix.Project)
                    => new MagicPath(
                        MagicPathType.Project,
                        s.Remove(0, Prefix.Project.Length),
                        stringValue),
                string s when s.StartsWith(Prefix.FileLinux)
                    => new MagicPath(
                        MagicPathType.File,
                        s.Remove(0, Prefix.FileLinux.Length),
                        stringValue),
                string s when s.StartsWith(Prefix.FileWindows)
                    => new MagicPath(
                        MagicPathType.File,
                        s.Remove(0, Prefix.FileWindows.Length),
                        stringValue),
                _ => null
            };
        }
        return null;
    }

    public string Replace(MagicPathContext context)
    {
        switch (_type)
        {
            case MagicPathType.Home:
                return Path.Combine(context.HomeDirectory.FullName, _path);
            case MagicPathType.Solution when context.SolutionDirectory is not null:
                return Path.Combine(context.SolutionDirectory.FullName, _path);
            case MagicPathType.Project when context.ProjectDirectory is not null:
                return Path.Combine(context.ProjectDirectory.FullName, _path);
            case MagicPathType.File:
                return Path.Combine(context.FileDirectory.FullName, _path);
            default:
                App.Log.NotSupportedInContext(_type);
                return _original;
        }
    }
}

file static class LogExtensions
{
    public static void NotSupportedInContext(this IConsoleLogger log, MagicPathType type)
        => log.Debug($"Magic path type {0} is not supported in this context.", type.ToString());

    public static void ReplacedMagicPath(this IConsoleLogger log, MagicPath magicPath, string replacedValue)
        => log.Debug($"Replaced magic path {0} with {1}.", magicPath, replacedValue);
}
