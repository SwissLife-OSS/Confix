using System.Diagnostics.CodeAnalysis;
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
    private readonly string[] _arguments;
    private readonly string _path;
    private readonly IGitService _git;

    public GitComponentProvider(
        IGitService git,
        JsonNode node)
        : this(git, GitComponentProviderConfiguration.Parse(node))
    {
    }

    public GitComponentProvider(
        IGitService git,
        GitComponentProviderConfiguration configuration)
        : this(git, GitComponentProviderDefinition.From(configuration))
    {
    }

    public GitComponentProvider(
        IGitService git,
        GitComponentProviderDefinition definition)
    {
        _git = git;
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
        var components = context.ComponentReferences.Where(x => x.Provider == _name && x.IsEnabled);

        var refs = await FetchRefsAsync(context);

        var tasks = components
            .Select(x => ProcessComponentAsync(x, refs, context))
            .ToArray();

        List<string>? errors = null;

        foreach (var task in tasks)
        {
            var result = (ComponentOrError) (await task)!;
            if (result.Component is not null)
            {
                context.Components.Add(result.Component);
            }
            else
            {
                errors ??= new();
                errors.Add(result.Error!);
            }
        }

        if (errors is not null)
        {
            throw new ExitException(
                $"Failed to process components from git repository '{_name}':\n{string.Join("\n", errors)}");
        }
    }

    private async Task<IReadOnlyDictionary<string, string>> FetchRefsAsync(
        IComponentProviderContext context)
    {
        var directory =
            new DirectoryInfo(Path.Combine(_cloneDirectory.FullName, "__refs"));
        var refs = new Dictionary<string, string>();
        
        string gitUrl = GitUrl.Create(_repositoryUrl, context.Parameter);

        var sparseConfig =
            new GitSparseCheckoutConfiguration(gitUrl, directory.FullName, _arguments);
        await _git.SparseCheckoutAsync(sparseConfig, context.CancellationToken);

        var showRefConfig = new GitShowRefsConfiguration(directory.FullName, _arguments);
        var refsOutput = await _git.ShowRefsAsync(showRefConfig, context.CancellationToken);

        foreach (var line in refsOutput.Split('\n'))
        {
            var parts = line.Split(' ');
            if (parts.Length != 2)
            {
                continue;
            }

            var hash = parts[0];
            var name = parts[1];

            refs[name] = hash;
        }

        return refs;
    }

    private async Task<ComponentOrError?> ProcessComponentAsync(
        ComponentReferenceDefinition definition,
        IReadOnlyDictionary<string, string> refs,
        IComponentProviderContext context)
    {
        var version = definition.Version ?? "latest";
        var componentName = definition.ComponentName;

        try
        {
            var directory =
                new DirectoryInfo(Path.Combine(_cloneDirectory.FullName, componentName));

            directory.EnsureFolder();

            var cloneArgument = _arguments.ToList();
            
            string gitUrl = GitUrl.Create(_repositoryUrl, context.Parameter);

            var cloneConfiguration = new GitCloneConfiguration(
                gitUrl,
                directory.FullName,
                cloneArgument.ToArray());

            await _git.CloneAsync(cloneConfiguration, context.CancellationToken);

            if (version is not "latest")
            {
                if (!refs.TryGetReference(version, out var hash))
                {
                    return new(
                        $"Could not find component {componentName} ({version}) in git repository");
                }

                await _git.CheckoutAsync(
                    new GitCheckoutConfiguration(directory.FullName, hash, cloneArgument.ToArray()),
                    context.CancellationToken);
            }

            var pathToComponent = Path
                .Combine(directory.FullName, _path, componentName, FileNames.Schema);

            if (!File.Exists(pathToComponent))
            {
                return new(
                    $"Could not find component {componentName} ({version}) in git repository");
            }

            context.Logger.FoundComponent(componentName, version);

            var json = JsonSchema.FromFile(pathToComponent);
            var component =
                new Component(_name, componentName, version, true, definition.MountingPoints, json);

            return new(component);
        }
        catch (Exception ex)
        {
            return new(
                $"Unexpected error while processing component {componentName} ({version}): {ex.Message}");
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

    private struct ComponentOrError
    {
        public Component? Component { get; }

        public string? Error { get; }

        public ComponentOrError(Component component)
        {
            Component = component;
            Error = null;
        }

        public ComponentOrError(string error)
        {
            Component = null;
            Error = error;
        }
    }
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

    public static bool TryGetReference(
        this IReadOnlyDictionary<string, string> refs,
        string name,
        [NotNullWhen(true)] out string? hash)
    {
        if (refs.TryGetValue("refs/heads/" + name, out hash))
        {
            return true;
        }

        if (refs.TryGetValue("refs/remotes/origin/" + name, out hash))
        {
            return true;
        }

        if (refs.TryGetValue("refs/tags/" + name, out hash))
        {
            return true;
        }

        return false;
    }
}
