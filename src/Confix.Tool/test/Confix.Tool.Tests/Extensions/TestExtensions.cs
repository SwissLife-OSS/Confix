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
        => ReplacePath(str, info.Directories.Content.Parent!.FullName, name);
    
    public static string ReplacePath(this string str, TestMiddlewareContext info, string name)
        => ReplacePath(str, info.Directories.Content.Parent!.FullName, name);

    private static string ReplacePath(string value, string path, string name)
    {
        var replacement = $"<<{name}>>";
        var result = value.Replace(path, replacement);

        if (Path.DirectorySeparatorChar == '\\')
        {
            var forwardSlashPath = path.Replace('\\', '/');
            result = result.Replace(forwardSlashPath, replacement);
            result = result.Replace(path.Replace("\\", "\\\\"), replacement);
            result = result.Replace(forwardSlashPath.Replace("/", "\\/"), replacement);
        }

        return result;
    }
}
