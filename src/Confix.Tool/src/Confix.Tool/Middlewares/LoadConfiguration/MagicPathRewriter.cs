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
            SchemaValueType.String when MagicPath.From(value) is { } magicPath
                => JsonValue.Create(magicPath.Replace(context)),
            _ => base.Rewrite(value!, context)
        };
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

    public MagicPath(MagicPathType type, string path)
    {
        _type = type;
        _path = path;
    }

    public static MagicPath? From(JsonValue value)
    {
        if ((string?)value is { } stringValue)
        {
            return stringValue switch
            {
                string s when s.StartsWith(Prefix.Home) => new MagicPath(MagicPathType.Home, s.Remove(0, Prefix.Home.Length)),
                string s when s.StartsWith(Prefix.Tilde) => new MagicPath(MagicPathType.Home, s.Remove(0, Prefix.Tilde.Length)),
                string s when s.StartsWith(Prefix.Solution) => new MagicPath(MagicPathType.Solution, s.Remove(0, Prefix.Solution.Length)),
                string s when s.StartsWith(Prefix.Project) => new MagicPath(MagicPathType.Project, s.Remove(0, Prefix.Project.Length)),
                string s when s.StartsWith(Prefix.FileLinux) => new MagicPath(MagicPathType.File, s.Remove(0, Prefix.FileLinux.Length)),
                string s when s.StartsWith(Prefix.FileWindows) => new MagicPath(MagicPathType.File, s.Remove(0, Prefix.FileWindows.Length)),
                _ => null
            };
        }
        return null;
    }

    public string Replace(MagicPathContext context)
        => _type switch
        {
            MagicPathType.Home => Path.Combine(context.HomeDirectory.FullName, _path),
            MagicPathType.Solution when context.SolutionDirectory is not null
                => Path.Combine(context.SolutionDirectory.FullName, _path),
            MagicPathType.Project when context.ProjectDirectory is not null
                => Path.Combine(context.ProjectDirectory.FullName, _path),
            MagicPathType.File => Path.Combine(context.FileDirectory.FullName, _path),
            _ => throw new ArgumentOutOfRangeException(
                nameof(_type),
                _type,
                "The magic path is not supported in this context")
        };
}
