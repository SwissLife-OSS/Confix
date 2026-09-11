using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Validation;

internal static class ConfigurationInput
{
    private const string EnvironmentPlaceholder = "{environment}";
    private const string SidecarSuffix = ".confix.json";

    public static async Task ApplyAsync(
        IMiddlewareContext context,
        ValidationConfiguration settings)
    {
        var directory = context.Features
            .Get<ConfigurationFeature>()
            .EnsureProject()
            .Directory!
            .FullName;

        var environment = context.Features.Get<EnvironmentFeature>().ActiveEnvironment.Name;

        foreach (var file in context.Features.Get<ConfigurationFileFeature>().Files)
        {
            RedirectInPlaceOutput(context, file);

            var content = await file.TryLoadContentAsync(context.CancellationToken)
                ?? throw new ExitException("Could not read configuration input.");

            foreach (var overlay in settings.Overlays ?? [])
            {
                var additional = await LoadOverlayAsync(
                    directory,
                    overlay,
                    environment,
                    context.CancellationToken);

                if (content is not JsonObject target || additional is not JsonObject source)
                {
                    throw new ExitException("Configuration and overlays must be JSON objects.");
                }

                Merge(target, source);
            }

            file.Content = content;
        }
    }

    /// <summary>Resolved values must not overwrite the source appsettings unless asked for.</summary>
    private static void RedirectInPlaceOutput(
        IMiddlewareContext context,
        ConfigurationFile file)
    {
        if (file.InputFile.FullName != file.OutputFile.FullName ||
            context.Parameter.TryGet(OutputFileOption.Instance, out FileInfo _))
        {
            return;
        }

        var name = Path.GetFileNameWithoutExtension(file.InputFile.Name) + SidecarSuffix;

        file.OutputFile = new FileInfo(Path.Combine(file.InputFile.DirectoryName!, name));
    }

    private static async Task<JsonNode?> LoadOverlayAsync(
        string directory,
        string overlay,
        string environment,
        CancellationToken cancellationToken)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        var relative = overlay.Replace(EnvironmentPlaceholder, environment);
        var path = Path.GetFullPath(Path.Combine(root, relative));

        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ExitException($"Overlay '{overlay}' must be inside the project directory.");
        }

        if (!File.Exists(path))
        {
            throw new ExitException($"Overlay '{overlay}' was declared but does not exist.");
        }

        try
        {
            return JsonNode.Parse(await File.ReadAllTextAsync(path, cancellationToken));
        }
        catch (Exception ex)
            when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            throw new ExitException($"Overlay '{overlay}' could not be read as JSON.");
        }
    }

    /// <summary>Objects merge by key; arrays, scalars and null replace the previous value.</summary>
    private static void Merge(JsonObject target, JsonObject source)
    {
        foreach (var (key, value) in source)
        {
            var existing = target.FirstOrDefault(
                property => property.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

            var name = existing.Key ?? key;

            if (target[name] is JsonObject targetObject && value is JsonObject sourceObject)
            {
                Merge(targetObject, sourceObject);
            }
            else
            {
                target[name] = value?.DeepClone();
            }
        }
    }
}
