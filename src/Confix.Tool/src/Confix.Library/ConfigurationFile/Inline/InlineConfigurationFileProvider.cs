using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Utilities.Json;

namespace Confix.Tool.Middlewares;

public sealed class InlineConfigurationFileProvider : IConfigurationFileProvider
{
    public static string Type => "inline";

    public Task<IReadOnlyList<ConfigurationFile>> GetConfigurationFilesAsync(
        IConfigurationFileContext context,
        CancellationToken ct)
    {
        var path = context.Definition.Value.ExpectValue<string>();

        if (context.Project.Directory is not { } directory)
        {
            return Task.FromResult<IReadOnlyList<ConfigurationFile>>(
                Array.Empty<ConfigurationFile>());
        }

        var files = new List<ConfigurationFile>();

        if (!Path.IsPathFullyQualified(path))
        {
            path = Path.Combine(directory.FullName, path);
        }

        context.Logger.FoundAInlineConfigurationFile(path);

        files.Add(new ConfigurationFile
        {
            InputFile = new FileInfo(path),
            OutputFile = new FileInfo(path)
        });

        return Task.FromResult<IReadOnlyList<ConfigurationFile>>(files);
    }
}

file static class Log
{
    public static void FoundAInlineConfigurationFile(this IConsoleLogger console, string file)
    {
        console.Debug($"Found a inline configuration file '{file}'");
    }
}
