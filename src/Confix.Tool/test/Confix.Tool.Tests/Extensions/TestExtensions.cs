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
    {
        var path = info.Directories.Content.Parent!.FullName;
        var result = str;
        
        // Replace original path
        result = result.Replace(path, $"<<{name}>>");
        
        // On Windows, also replace forward-slash version and JSON-escaped versions
        if (Path.DirectorySeparatorChar == '\\')
        {
            var forwardSlashPath = path.Replace('\\', '/');
            result = result.Replace(forwardSlashPath, $"<<{name}>>");
            
            // Handle JSON-escaped paths (backslashes doubled)
            var jsonEscapedBackslash = path.Replace("\\", "\\\\");
            result = result.Replace(jsonEscapedBackslash, $"<<{name}>>");
            
            // Handle JSON-escaped forward slash paths
            var jsonEscapedForward = forwardSlashPath.Replace("/", "\\/");
            result = result.Replace(jsonEscapedForward, $"<<{name}>>");
        }
        
        return result;
    }

    public static string ReplacePath(this string str, TestMiddlewareContext info, string name)
    {
        var path = info.Directories.Content.Parent!.FullName;
        var result = str;
        
        // Replace original path
        result = result.Replace(path, $"<<{name}>>");
        
        // On Windows, also replace forward-slash version and JSON-escaped versions
        if (Path.DirectorySeparatorChar == '\\')
        {
            var forwardSlashPath = path.Replace('\\', '/');
            result = result.Replace(forwardSlashPath, $"<<{name}>>");
            
            // Handle JSON-escaped paths (backslashes doubled)
            var jsonEscapedBackslash = path.Replace("\\", "\\\\");
            result = result.Replace(jsonEscapedBackslash, $"<<{name}>>");
            
            // Handle JSON-escaped forward slash paths
            var jsonEscapedForward = forwardSlashPath.Replace("/", "\\/");
            result = result.Replace(jsonEscapedForward, $"<<{name}>>");
        }
        
        return result;
    }
}
