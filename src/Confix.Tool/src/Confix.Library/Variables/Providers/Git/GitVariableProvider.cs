using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Utilities;
using static Confix.Utilities.GitHelpers;

namespace Confix.Variables;

public sealed class GitVariableProvider : IVariableProvider
{
    private readonly GitVariableProviderDefinition _definition;
    private readonly LocalVariableProvider _localVariableProvider;
    private readonly string _cloneDirectory;

    public GitVariableProvider(JsonNode configuration)
        : this(GitVariableProviderConfiguration.Parse(configuration))
    {
    }

    public GitVariableProvider(GitVariableProviderConfiguration configuration)
        : this(GitVariableProviderDefinition.From(configuration))
    {
    }

    public GitVariableProvider(GitVariableProviderDefinition definition)
    {
        _definition = definition;
        _cloneDirectory = GetCloneDirectory(_definition);

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

        await AddAsync(new GitAddConfiguration(_cloneDirectory, _definition.Arguments), ct);

        var commitMessage = $"Update {path} in {_definition.FilePath}";
        await CommitAsync(
            new GitCommitConfiguration(_cloneDirectory, commitMessage, _definition.Arguments),
            ct);

        await PushAsync(new GitPushConfiguration(_cloneDirectory, _definition.Arguments), ct);

        return result;
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_cloneDirectory))
        {
            Directory.Delete(_cloneDirectory, true);
        }

        return ValueTask.CompletedTask;
    }

    private async Task EnsureClonedAsync(bool forcePull, CancellationToken ct)
    {
        if (Directory.Exists(_cloneDirectory))
        {
            if (!forcePull)
            {
                return;
            }

            await PullAsync(new GitPullConfiguration(_cloneDirectory, _definition.Arguments), ct);
        }
        else
        {
            GitCloneConfiguration configuration =
                new(_definition.RepositoryUrl, _cloneDirectory, _definition.Arguments);

            await CloneAsync(configuration, ct);
        }
    }

    private static string GetCloneDirectory(GitVariableProviderDefinition providerDefinition)
        => Path.Combine(Path.GetTempPath(), ".confix", "git", Guid.NewGuid().ToString());
}

file static class Extensions
{
    public static string GetMd5Hash(this object obj)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj)));
        return Convert.ToHexString(hash);
    }
}
