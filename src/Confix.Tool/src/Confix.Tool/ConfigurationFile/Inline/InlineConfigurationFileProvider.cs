using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Utilities.Json;

namespace Confix.Tool.Middlewares;

public sealed class InlineConfigurationFileProvider : IConfigurationFileProvider
{
    public static string Type => "inline";

    public IReadOnlyList<ConfigurationFile> GetConfigurationFiles(IConfigurationFileContext context)
    {
        var files = new List<ConfigurationFile>();

        var path = context.Definition.Value.ExpectValue<string>();

        foreach (var file in context.Project.Directory!.FindAllInPath(path, false))
        {
            App.Log.FoundAInlineConfigurationFile(file);

            files.Add(new ConfigurationFile { File = file });
        }

        return files;
    }
}

file static class Log
{
    public static void FoundAInlineConfigurationFile(this IConsoleLogger console, FileInfo file)
    {
        console.Debug($"Found a inline configuration file '{file}'");
    }
}
