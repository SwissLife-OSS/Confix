using System.CommandLine;
using System.Text;
using System.Text.Json.Nodes;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;
using Confix.Tool.Middlewares.Encryption;

namespace Confix.Tool.Validation;

public sealed record ConfigurationReadOnlyFeature(bool ReadOnly);

public static class ConfigurationValidationPipeline
{
    public static readonly Option<bool> ExportSchemaOption = new("--export-schema") { Description = "Export an IDE schema for a code-first project." };

    public static async Task DispatchAsync(IMiddlewareContext context, MiddlewareDelegate next, bool write)
    {
        await new Pipeline(b => b.UseEnvironment()).ExecuteAsync(context);
        var settings = Settings(context);
        if ((settings?.EffectiveType ?? "json-schema") == "json-schema") { await next(context); return; }
        context.Features.Set(new ConfigurationReadOnlyFeature(!write));
        await new Pipeline(b => b.UseReadConfigurationFiles().UseEnvironment().Use(ApplyOverlaysAsync).Use<VariableMiddleware>()
            .Use<BuildProjectMiddleware>().Use((c, _) => ValidateAsync(c, write))).ExecuteAsync(context);
    }

    private static async Task ValidateAsync(IMiddlewareContext context, bool write)
    {
        var project = context.Features.Get<ConfigurationFeature>().EnsureProject();
        var directory = project.Directory!.FullName;
        var settings = Settings(context)!;
        var files = context.Features.Get<ConfigurationFileFeature>().Files;
        if (files.Count == 0) throw new ExitException("Configuration validation requires input.");
        var export = write && (ExportEnabled(context) ||
            (context.Parameter.TryGet(ExportSchemaOption, out bool requested) && requested));
        IConfigurationValidator validator = settings.EffectiveType switch
        {
            "dotnet-options" => new DotnetOptionsValidator(),
            _ => throw new ExitException("Unsupported configuration validation provider: " + settings.EffectiveType)
        };
        var schema = await validator.ValidateAsync(context, settings, export);
        if (write)
        {
            await new Pipeline(b => b.Use<OptionalEncryptionMiddleware>().Use((c, _) => PublishAsync(c))).ExecuteAsync(context);
            if (export && schema is not null)
            {
                var schemaPath = Path.Combine(directory, "confix.ide.schema.json");
                await File.WriteAllTextAsync(schemaPath, schema.ToJsonString(new() { WriteIndented = true }), context.CancellationToken);
                await AssociateSchemaAsync(directory, schemaPath, context.CancellationToken);
                context.Logger.Information("Exported confix.ide.schema.json.");
            }
        }
        context.Logger.Information("Configuration validation succeeded.");
    }

    private static async Task ApplyOverlaysAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var settings = Settings(context)!;
        var directory = context.Features.Get<ConfigurationFeature>().EnsureProject().Directory!.FullName;
        var environment = context.Features.Get<EnvironmentFeature>().ActiveEnvironment.Name;
        foreach (var file in context.Features.Get<ConfigurationFileFeature>().Files)
        {
            if (file.InputFile.FullName == file.OutputFile.FullName &&
                !context.Parameter.TryGet(OutputFileOption.Instance, out FileInfo explicitOutput))
                file.OutputFile = new FileInfo(Path.Combine(file.InputFile.DirectoryName!,
                    Path.GetFileNameWithoutExtension(file.InputFile.Name) + ".confix.json"));
            var content = await file.TryLoadContentAsync(context.CancellationToken)
                ?? throw new ExitException("Could not read configuration input.");
            foreach (var overlay in settings.Overlays ?? [])
            {
                var path = Path.GetFullPath(Path.Combine(directory, overlay.Replace("{environment}", environment)));
                var additional = JsonNode.Parse(await File.ReadAllTextAsync(path, context.CancellationToken));
                if (content is not JsonObject target || additional is not JsonObject source)
                    throw new ExitException("Configuration and overlays must be JSON objects.");
                Merge(target, source);
            }
            file.Content = content;
        }
        await next(context);

        static void Merge(JsonObject target, JsonObject source)
        {
            foreach (var (key, value) in source)
            {
                var existing = target.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                var name = existing.Key ?? key;
                if (target[name] is JsonObject a && value is JsonObject b) Merge(a, b);
                else target[name] = value?.DeepClone();
            }
        }
    }

    private static async Task PublishAsync(IMiddlewareContext context)
    {
        var staged = new List<(string Temporary, string Destination)>();
        try
        {
            foreach (var file in context.Features.Get<ConfigurationFileFeature>().Files)
            {
                if (staged.Any(s => s.Destination == file.OutputFile.FullName))
                    throw new ExitException("Multiple configuration inputs cannot share one output destination.");
                var bytes = Encoding.UTF8.GetBytes(file.Content!.ToJsonString(new() { WriteIndented = true }));
                if (context.Parameter.TryGet(EncryptionOption.Instance, out bool encrypted) && encrypted)
                {
                    if (!context.Features.TryGet<EncryptionFeature>(out var encryption))
                        throw new ExitException("Encryption must be configured before writing encrypted output.");
                    bytes = await encryption.EncryptionProvider.EncryptAsync(bytes, context.CancellationToken);
                }
                var path = file.OutputFile.FullName;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                staged.Add((temporary, path));
                var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write, Options = FileOptions.Asynchronous };
                if (!OperatingSystem.IsWindows()) options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
                await using var stream = new FileStream(temporary, options);
                await stream.WriteAsync(bytes, context.CancellationToken);
            }
            context.CancellationToken.ThrowIfCancellationRequested();
            foreach (var (temporary, destination) in staged) File.Move(temporary, destination, true);
        }
        finally { foreach (var (temporary, _) in staged) File.Delete(temporary); }
    }

    private static async Task AssociateSchemaAsync(string directory, string schemaPath, CancellationToken ct)
    {
        var path = Path.Combine(directory, ".vscode", "settings.json");
        var settings = File.Exists(path) ? JsonNode.Parse(await File.ReadAllTextAsync(path, ct),
            documentOptions: new() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, AllowTrailingCommas = true })!.AsObject() : new JsonObject();
        var schemas = settings["json.schemas"] as JsonArray ?? new JsonArray();
        if (schemas.Parent is null) settings["json.schemas"] = schemas;
        var url = "./" + Path.GetFileName(schemaPath);
        if (!schemas.Any(s => s?["url"]?.GetValue<string>() == url))
            schemas.Add(new JsonObject { ["fileMatch"] = new JsonArray("/appsettings*.json"), ["url"] = url });
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, settings.ToJsonString(new() { WriteIndented = true }), ct);
    }

    private static bool ExportEnabled(IMiddlewareContext context)
    {
        var project = context.Features.Get<ConfigurationFeature>().EnsureProject();
        var environment = context.Features.Get<EnvironmentFeature>().ActiveEnvironment;
        return environment.ExportSchema ?? project.ExportSchema ?? false;
    }

    private static Confix.Tool.Abstractions.ValidationConfiguration? Settings(IMiddlewareContext context)
    {
        var project = context.Features.Get<ConfigurationFeature>().EnsureProject();
        var environment = context.Features.Get<EnvironmentFeature>().ActiveEnvironment;
        return project.Validation?.Merge(environment.Validation) ?? environment.Validation;
    }

}
