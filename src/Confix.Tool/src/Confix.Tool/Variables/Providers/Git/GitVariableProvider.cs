using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public sealed class GitVariableProvider : IVariableProvider
{
    private readonly GitVariableProviderDefinition _definition;
    private readonly LocalVariableProvider _localVariableProvider;
    private readonly string _cloneDirectory;

    public GitVariableProvider(JsonNode configuration)
        : this(GitVariableProviderConfiguration.Parse(configuration))
    { }

    public GitVariableProvider(GitVariableProviderConfiguration configuration)
        : this(GitVariableProviderDefinition.From(configuration))
    { }

    public GitVariableProvider(GitVariableProviderDefinition definition)
    {
        _definition = definition;
        _cloneDirectory = GetCloneDirectory(_definition);
        _localVariableProvider = new LocalVariableProvider(new LocalVariableProviderConfiguration(
            Path.Combine(_cloneDirectory, _definition.FilePath)
        ));
    }

    public async Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
    {
        await EnsureCloned(cancellationToken);
        return await _localVariableProvider.ListAsync(cancellationToken);
    }

    public async Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        await EnsureCloned(cancellationToken);
        return await _localVariableProvider.ResolveAsync(path, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken)
    {
        await EnsureCloned(cancellationToken);
        return await _localVariableProvider.ResolveManyAsync(paths, cancellationToken);
    }

    public async Task<string> SetAsync(string path, JsonNode value, CancellationToken cancellationToken)
    {
        await EnsureCloned(cancellationToken);
        return await _localVariableProvider.SetAsync(path, value, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        Directory.Delete(_cloneDirectory, true);
        return ValueTask.CompletedTask;
    }

    private static string GetCloneDirectory(GitVariableProviderDefinition providerDefinition)
        => Path.Combine(Path.GetTempPath(), ".confix", "git", providerDefinition.GetMd5Hash());

    private async Task EnsureCloned(CancellationToken cancellationToken)
    {
        if (Directory.Exists(_cloneDirectory))
        {
            return;
        }

        GitCloneConfiguration configuration = new(
            _definition.RepositoryUrl,
            _cloneDirectory,
            _definition.Arguments
        );

        await GitHelpers.Clone(configuration, cancellationToken);
    }
}

file static class Extensions
{
    public static string GetMd5Hash(this object obj)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj)));
        return Convert.ToHexString(hash);
    }
}