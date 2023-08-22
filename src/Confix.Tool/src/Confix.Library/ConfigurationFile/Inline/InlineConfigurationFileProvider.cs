using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Utilities.Json;

namespace Confix.Tool.Middlewares;

public sealed class InlineConfigurationFileProvider : IConfigurationFileProvider
{
    public static string Type => "inline";

    public IReadOnlyList<ConfigurationFile> GetConfigurationFiles(IConfigurationFileContext context)
    {
        var path = context.Definition.Value.ExpectValue<string>();

        if (context.Project.Directory is not { } directory)
        {
            return Array.Empty<ConfigurationFile>();
        }

        var files = new List<ConfigurationFile>();

        foreach (var file in directory.FindAllInPath(path, false))
        {
            context.Logger.FoundAInlineConfigurationFile(file);

            files.Add(new ConfigurationFile { InputFile = file, OutputFile = file });
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
