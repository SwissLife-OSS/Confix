using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Encryption;

namespace Confix.Tool.Validation;

internal static class ConfigurationOutput
{
    public const string SchemaFileName = "confix.ide.schema.json";

    private const string SchemaSetting = "json.schemas";

    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };

    private static readonly JsonNodeOptions _nodeOptions = new();

    private static readonly JsonDocumentOptions _documentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>Writes every output to a sibling temporary file before replacing the destinations.</summary>
    public static async Task PublishAsync(IMiddlewareContext context)
    {
        var staged = new List<(string Temporary, string Destination)>();

        try
        {
            foreach (var file in context.Features.Get<ConfigurationFileFeature>().Files)
            {
                var destination = file.OutputFile.FullName;

                if (staged.Any(s => s.Destination == destination))
                {
                    throw new ExitException(
                        "Multiple configuration inputs cannot share one output destination.");
                }

                var bytes = await EncryptAsync(context, file);
                var temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";

                staged.Add((temporary, destination));

                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                await WriteAsync(temporary, bytes, context.CancellationToken);
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

    public static async Task ExportSchemaAsync(
        string directory,
        JsonNode schema,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, SchemaFileName);

        await File.WriteAllTextAsync(path, schema.ToJsonString(_indented), cancellationToken);
        await AssociateSchemaAsync(directory, path, cancellationToken);
    }

    private static async Task<byte[]> EncryptAsync(
        IMiddlewareContext context,
        ConfigurationFile file)
    {
        var bytes = Encoding.UTF8.GetBytes(file.Content!.ToJsonString(_indented));

        if (!context.Parameter.TryGet(EncryptionOption.Instance, out bool encrypted) || !encrypted)
        {
            return bytes;
        }

        if (!context.Features.TryGet<EncryptionFeature>(out var encryption))
        {
            throw new ExitException(
                "Encryption must be configured before writing encrypted output.");
        }

        return await encryption.EncryptionProvider.EncryptAsync(bytes, context.CancellationToken);
    }

    private static async Task WriteAsync(
        string path,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        var options = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Options = FileOptions.Asynchronous
        };

        // Resolved output can contain secrets, so it is never world readable.
        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        await using var stream = new FileStream(path, options);
        await stream.WriteAsync(bytes, cancellationToken);
    }

    private static async Task AssociateSchemaAsync(
        string directory,
        string schemaPath,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, ".vscode", "settings.json");
        var settings = await ReadSettingsAsync(path, cancellationToken);

        if (settings[SchemaSetting] is not JsonArray schemas)
        {
            schemas = [];
            settings[SchemaSetting] = schemas;
        }

        var url = "./" + Path.GetFileName(schemaPath);

        if (!schemas.Any(s => s?["url"]?.GetValue<string>() == url))
        {
            schemas.Add(new JsonObject
            {
                ["fileMatch"] = new JsonArray("/appsettings*.json"),
                ["url"] = url
            });
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, settings.ToJsonString(_indented), cancellationToken);
    }

    private static async Task<JsonObject> ReadSettingsAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return new JsonObject();
        }

        var content = await File.ReadAllTextAsync(path, cancellationToken);

        return JsonNode.Parse(content, _nodeOptions, _documentOptions)?.AsObject()
            ?? new JsonObject();
    }
}
