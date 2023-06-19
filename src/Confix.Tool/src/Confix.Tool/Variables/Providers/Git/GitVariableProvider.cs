using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public sealed class GitVariableProvider : IVariableProvider
{
    private readonly GitVariableProviderConfiguration _configuration;
    private readonly LocalVariableProvider _localVariableProvider;
    private readonly string _cloneDirectory;

    public GitVariableProvider(JsonNode configuration)
        : this(GitVariableProviderConfiguration.Parse(configuration))
    { }

    public GitVariableProvider(GitVariableProviderConfiguration configuration)
    {
        _configuration = configuration;
        _cloneDirectory = GetCloneDirectory(configuration);
        _localVariableProvider = new LocalVariableProvider(new LocalVariableProviderConfiguration
        {
            FilePath = Path.Combine(_cloneDirectory, configuration.FilePath)
        });
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

    private static string GetCloneDirectory(GitVariableProviderConfiguration providerConfiguration)
        => Path.Combine(Path.GetTempPath(), ".confix", "git", providerConfiguration.GetMd5Hash());

    private async Task EnsureCloned(CancellationToken cancellationToken)
    {
        if (Directory.Exists(_cloneDirectory))
        {
            return;
        }

        GitCloneConfiguration configuration = new(
            _configuration.RepositoryUrl,
            _cloneDirectory,
            _configuration.Arguments
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