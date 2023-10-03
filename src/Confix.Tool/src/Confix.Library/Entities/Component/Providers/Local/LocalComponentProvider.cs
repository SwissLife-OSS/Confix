using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Schema;
using Confix.Utilities.FileSystem;
using Json.Schema;

namespace Confix.Tool.Entities.Components.Local;

public sealed class LocalComponentProvider : IComponentProvider, IAsyncDisposable
{
    public static string Name => "__LOCAL";

    public static string Type => "local";

    /// <inheritdoc />
    public async Task ExecuteAsync(IComponentProviderContext context)
    {
        var componentFolder = context.Project.Directory!
            .Append(FolderNames.Confix)
            .Append(FolderNames.Components);

        var references = context.ComponentReferences
            .Where(x => x.Provider == Name && x.IsEnabled)
            .ToList();

        if (!Directory.Exists(componentFolder.FullName))
        {
            return;
        }

        foreach (var directory in componentFolder.GetDirectories())
        {
            var componentFile = directory.Append(FileNames.ConfixComponent);
            if (!File.Exists(componentFile.FullName))
            {
                context.Logger.SkippedComponentBecauseMissingComponentFile(directory);
                continue;
            }

            await using var stream = File.OpenRead(componentFile.FullName);
            var configuration = ComponentConfiguration.Parse(JsonNode.Parse(stream));
            var name = configuration.Name ?? directory.Name;
            context.Logger.FoundComponent(name);

            var schemaFile = directory.Append(FileNames.Schema);
            if (!File.Exists(schemaFile.FullName))
            {
                throw new ExitException(
                    $"The {FileNames.Schema} file of the component '{name}' is missing.")
                {
                    Help =
                        $"Run `confix build` in the project directory or the component directory to generate the {FileNames.Schema} file."
                };
            }

            if (context.Components.FirstOrDefault(x => x.ComponentName == name) is
                { Provider: { } provider })
            {
                context.Logger.SkippedComponentBecauseItWasAlreadyDiscoveredBy(name, provider);
                continue;
            }

            var schema = JsonSchema.FromFile(schemaFile.FullName);
            var reference = references.FirstOrDefault(x => x.ComponentName == name);

            context.Components.Add(new Component(
                Name,
                name,
                "latest",
                reference?.IsEnabled ?? true,
                reference?.MountingPoints ?? new List<string>() { name },
                schema));

            if (reference is not null)
            {
                references.Remove(reference);
            }
        }

        if (references.Any())
        {
            throw new ExitException(
                $"The following components were not found in the local component provider: {string.Join(", ", references.Select(x => x.ComponentName))}");
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return default;
    }
}

public static class LoggerExtensions
{
    public static void FoundComponent(
        this IConsoleLogger logger,
        string componentName)
    {
        logger.Debug($"Found component {componentName} in provider {LocalComponentProvider.Name}");
    }

    public static void SkippedComponentBecauseMissingComponentFile(
        this IConsoleLogger logger,
        DirectoryInfo directory)
    {
        logger.Debug(
            $"Skipped component {directory.Name} because the {FileNames.ConfixComponent} file is missing");
    }

    public static void SkippedComponentBecauseItWasAlreadyDiscoveredBy(
        this IConsoleLogger logger,
        string componentName,
        string provider)
    {
        logger.Debug(
            $"Skipped component {componentName} because it was already discovered by {provider}");
    }
}
