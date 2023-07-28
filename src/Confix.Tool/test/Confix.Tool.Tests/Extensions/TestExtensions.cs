using System.Text.Json;
using ConfiX.Entities.Component.Configuration.Middlewares;
using ConfiX.Inputs;

namespace ConfiX.Entities.Component.Configuration;

public static class TestExtensions
{
    public static string ToJsonString(this object node)
        => JsonSerializer.Serialize(node,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

    public static string ReplacePath(this string str, TestConfixCommandline info, string name)
        => str.Replace(info.Directories.Content.Parent!.FullName, $"<<{name}>>");
    
    public static string ReplacePath(this string str, TestMiddlewareContext info, string name)
        => str.Replace(info.Directories.Content.Parent!.FullName, $"<<{name}>>");
}
