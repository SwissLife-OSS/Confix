using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

public sealed record JsonFile
(
    FileInfo File,
    JsonNode Content
)
{
    public static async ValueTask<JsonFile> FromFile(FileInfo file, CancellationToken cancellationToken)
    {
        var content = await file.ReadAllText(cancellationToken);
        var json = JsonNode.Parse(content);

        if (json is null)
        {
            App.Log.InvalidContent(file);
            throw new InvalidOperationException($"File {file.FullName} has invalid content.");
        }

        return new(file, json);
    }
}

file static class Log
{
    public static void InvalidContent(this IConsoleLogger logger, FileInfo file)
    {
        logger.Error($"File {file.FullName} has invalid content.");
    }
}
