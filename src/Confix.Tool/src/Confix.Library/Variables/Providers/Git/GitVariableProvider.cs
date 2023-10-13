using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Utilities;

namespace Confix.Variables;

public sealed class GitVariableProvider : IVariableProvider
{
    private readonly GitVariableProviderDefinition _definition;
    private readonly LocalVariableProvider _localVariableProvider;
    private readonly string _cloneDirectory;
    private readonly IGitService _git;

    public GitVariableProvider(
        IGitService git,
        JsonNode configuration)
        : this(git, GitVariableProviderConfiguration.Parse(configuration))
    {
    }

    public GitVariableProvider(
        IGitService git,
        GitVariableProviderConfiguration configuration)
        : this(git, GitVariableProviderDefinition.From(configuration))
    {
    }

    public GitVariableProvider(
        IGitService git,
        GitVariableProviderDefinition definition)
    {
        _git = git;
        _definition = definition;
        _cloneDirectory = GetCloneDirectory();

        var tempPath = Path.Combine(_cloneDirectory, _definition.FilePath);
        var localDefinition = new LocalVariableProviderDefinition(tempPath);
        _localVariableProvider = new LocalVariableProvider(localDefinition);
    }

    public static string Type => "git";

    public async Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
    {
        await EnsureClonedAsync(true, cancellationToken);
        return await _localVariableProvider.ListAsync(cancellationToken);
    }

    public async Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        await EnsureClonedAsync(true, cancellationToken);
        return await _localVariableProvider.ResolveAsync(path, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
    {
        await EnsureClonedAsync(true, cancellationToken);
        return await _localVariableProvider.ResolveManyAsync(paths, cancellationToken);
    }

    public async Task<string> SetAsync(
        string path,
        JsonNode value,
        CancellationToken ct)
    {
        await EnsureClonedAsync(true, ct);
        var result = await _localVariableProvider.SetAsync(path, value, ct);

        await _git.AddAsync(new GitAddConfiguration(_cloneDirectory, _definition.Arguments), ct);

        var commitMessage = $"Update {path} in {_definition.FilePath}";
        await _git.CommitAsync(
            new GitCommitConfiguration(_cloneDirectory, commitMessage, _definition.Arguments),
            ct);

        await _git.PushAsync(new GitPushConfiguration(_cloneDirectory, _definition.Arguments), ct);

        return result;
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_cloneDirectory))
        {
            Directory
                .EnumerateFiles(_cloneDirectory, "*", SearchOption.AllDirectories)
                .ForEach(DeleteFile);
        }

        return ValueTask.CompletedTask;
    }

    private static void DeleteFile(string file)
    {
        var fileInfo = new FileInfo(file);
        if (!fileInfo.Exists)
        {
            return;
        }

        if (fileInfo.IsReadOnly)
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        File.Delete(file);
    }

    private async Task EnsureClonedAsync(bool forcePull, CancellationToken ct)
    {
        if (Directory.Exists(_cloneDirectory))
        {
            if (!forcePull)
            {
                return;
            }

            await _git.PullAsync(new(_cloneDirectory, _definition.Arguments), ct);
        }
        else
        {
            GitCloneConfiguration configuration =
                new(_definition.RepositoryUrl, _cloneDirectory, _definition.Arguments);

            await _git.CloneAsync(configuration, ct);
        }
    }

    private static string GetCloneDirectory()
        => Path.Combine(Path.GetTempPath(), ".confix", "git", Guid.NewGuid().ToString());
}
