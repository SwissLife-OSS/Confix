using System.Text;
using System.Text.Json.Nodes;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Encryption;

namespace Confix.Tool.Validation;

internal static class ConfigurationOutput
{
    public static async Task PublishAsync(IMiddlewareContext context)
    {
        var staged = new List<(string Temporary, string Destination)>();
        try
        {
            foreach (var file in context.Features.Get<ConfigurationFileFeature>().Files)
            {
                if (staged.Any(s => s.Destination == file.OutputFile.FullName))
                {
                    throw new ExitException("Multiple configuration inputs cannot share one output destination.");
                }
                var bytes = Encoding.UTF8.GetBytes(file.Content!.ToJsonString(new() { WriteIndented = true }));
                if (context.Parameter.TryGet(EncryptionOption.Instance, out bool encrypted) && encrypted)
                {
                    if (!context.Features.TryGet<EncryptionFeature>(out var encryption))
                    {
                        throw new ExitException("Encryption must be configured before writing encrypted output.");
                    }
                    bytes = await encryption.EncryptionProvider.EncryptAsync(bytes, context.CancellationToken);
                }
                var path = file.OutputFile.FullName;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                staged.Add((temporary, path));
                var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write, Options = FileOptions.Asynchronous };
                if (!OperatingSystem.IsWindows())
                {
                    options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
                }
                await using var stream = new FileStream(temporary, options);
                await stream.WriteAsync(bytes, context.CancellationToken);
            }
            context.CancellationToken.ThrowIfCancellationRequested();
            foreach (var (temporary, destination) in staged)
            {
                File.Move(temporary, destination, true);
            }
        }
        finally
        {
            foreach (var (temporary, _) in staged)
            {
                File.Delete(temporary);
            }
        }
    }

    public static async Task ExportSchemaAsync(string directory, JsonNode schema, CancellationToken cancellationToken)
    {
        var schemaPath = Path.Combine(directory, "confix.ide.schema.json");
        await File.WriteAllTextAsync(schemaPath, schema.ToJsonString(new() { WriteIndented = true }), cancellationToken);
        await AssociateSchemaAsync(directory, schemaPath, cancellationToken);
    }

    private static async Task AssociateSchemaAsync(string directory, string schemaPath, CancellationToken ct)
    {
        var path = Path.Combine(directory, ".vscode", "settings.json");
        var settings = File.Exists(path) ? JsonNode.Parse(await File.ReadAllTextAsync(path, ct),
            documentOptions: new() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, AllowTrailingCommas = true })!.AsObject() : new JsonObject();
        var schemas = settings["json.schemas"] as JsonArray ?? new JsonArray();
        if (schemas.Parent is null)
        {
            settings["json.schemas"] = schemas;
        }
        var url = "./" + Path.GetFileName(schemaPath);
        if (!schemas.Any(s => s?["url"]?.GetValue<string>() == url))
        {
            schemas.Add(new JsonObject { ["fileMatch"] = new JsonArray("/appsettings*.json"), ["url"] = url });
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, settings.ToJsonString(new() { WriteIndented = true }), ct);
    }

}
