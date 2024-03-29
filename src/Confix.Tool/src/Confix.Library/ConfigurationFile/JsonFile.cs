using System.Text.Json.Nodes;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

public sealed record JsonFile(FileInfo File, JsonNode Content)
{
    public static async ValueTask<JsonFile> FromFile(
        FileInfo file,
        CancellationToken cancellationToken)
    {
        var content = await file.ReadAllText(cancellationToken);
        var json = JsonNode.Parse(content);

        if (json is null)
        {
            throw ThrowHelper.CouldNotParseJsonFile(file);
        }

        return new(file, json);
    }
}
