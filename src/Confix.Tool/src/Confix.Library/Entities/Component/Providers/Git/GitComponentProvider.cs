using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;
using Confix.Utilities;
using Json.Schema;
using static System.Environment.SpecialFolder;
using static System.Environment.SpecialFolderOption;

namespace Confix.Tool.Entities.Components.Git;

public sealed class GitComponentProvider : IComponentProvider, IAsyncDisposable
{
    private readonly DirectoryInfo _cloneDirectory;
    private readonly string _name;
    private readonly string _repositoryUrl;
    private readonly IReadOnlyList<string> _arguments;
    private readonly string _path;

    public GitComponentProvider(JsonNode node)
        : this(GitComponentProviderConfiguration.Parse(node))
    {
    }

    public GitComponentProvider(GitComponentProviderConfiguration configuration)
        : this(GitComponentProviderDefinition.From(configuration))
    {
    }

    public GitComponentProvider(GitComponentProviderDefinition definition)
    {
        _name = definition.Name;
        _repositoryUrl = definition.RepositoryUrl;
        _arguments = definition.Arguments;
        _path = definition.Path;
        _cloneDirectory = GetCloneDirectory(definition);
    }

    public static string Type => "git";

    /// <inheritdoc />
    public async Task ExecuteAsync(IComponentProviderContext context)
    {
        var components = context.Components.Where(x => x.Provider == _name && x.IsEnabled);

        var errors = new ConcurrentBag<string>();

        await Task.WhenAll(components.Select(x
            => ProcessComponentAsync(x, errors, context.Logger, context.CancellationToken)));

        if (errors.Count > 0)
        {
            throw new ExitException(
                $"Failed to process components from git repository '{_name}':\n{string.Join("\n", errors)}");
        }
    }

    private async Task ProcessComponentAsync(
        Component component,
        ConcurrentBag<string> errors,
        IConsoleLogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            component.Version ??= "latest";
            var directory =
                new DirectoryInfo(Path.Combine(_cloneDirectory.FullName, component.ComponentName));

            directory.EnsureFolder();

            var cloneArgument = _arguments.ToList();
            if (component.Version != "latest")
            {
                cloneArgument.Add($"--branch={component.Version}");
            }

            var cloneConfiguration =
                new GitCloneConfiguration(_repositoryUrl,
                    directory.FullName,
                    cloneArgument.ToArray());

            await GitHelpers.CloneAsync(cloneConfiguration, cancellationToken);

            var pathToComponent = Path
                .Combine(directory.FullName, _path, component.ComponentName, FileNames.Schema);

            if (!File.Exists(pathToComponent))
            {
                errors.Add(
                    $"Could not find component {component.ComponentName} ({component.Version}) in git repository");
            }

            logger.FoundComponent(component.ComponentName, component.Version);

            component.Schema = JsonSchema.FromFile(pathToComponent);
        }
        catch (Exception ex)
        {
            errors.Add(
                $"Unexpected error while processing component {component.ComponentName} ({component.Version}): {ex.Message}");
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _cloneDirectory.Refresh();
        if (_cloneDirectory.Exists)
        {
            _cloneDirectory.Delete(true);
        }

        return ValueTask.CompletedTask;
    }

    private static DirectoryInfo GetCloneDirectory(GitComponentProviderDefinition definition)
        => new(Path.Combine(
            Environment.GetFolderPath(ApplicationData, Create),
            ".confix",
            "git",
            definition.Name,
            Guid.NewGuid().ToString()));
}

public static class LoggerExtensions
{
    public static void FoundComponent(
        this IConsoleLogger logger,
        string componentName,
        string version)
    {
        logger.Debug($"Found component {componentName} ({version})");
    }
}
