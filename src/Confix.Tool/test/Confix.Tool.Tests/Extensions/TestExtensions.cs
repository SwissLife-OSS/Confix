using System.Text.Json;
using Confix.Entities.Component.Configuration.Middlewares;
using Confix.Inputs;

namespace Confix.Entities.Component.Configuration;

public static class TestExtensions
{
    public static string ToJsonString(this object node)
        => JsonSerializer.Serialize(node,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

    public static string ReplacePath(this string str, TestConfixCommandline info, string name)
        => str
            .Replace(info.Directories.Content.Parent!.FullName, $"<<{name}>>")
            .NormalizePathSeparators();

    public static string ReplacePath(this string str, TestMiddlewareContext info, string name)
        => str
            .Replace(info.Directories.Content.Parent!.FullName, $"<<{name}>>")
            .NormalizePathSeparators();

    /// <summary>
    /// Normalizes path separators to forward slashes for consistent cross-platform snapshots.
    /// </summary>
    public static string NormalizePathSeparators(this string str)
        => str.Replace('\\', '/');
}
