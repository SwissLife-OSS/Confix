using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace ConfiX.Variables;

public sealed class LocalVariableProvider : IVariableProvider
{
    private readonly Lazy<Dictionary<string, JsonNode?>> _configuration;

    public LocalVariableProvider(JsonNode configuration)
        : this(LocalVariableProviderConfiguration.Parse(configuration))
    { }

    public LocalVariableProvider(LocalVariableProviderConfiguration configuration)
    {
        _configuration = new(() => ParseConfiguration(configuration));
    }

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<string>>(_configuration.Value.Keys.ToArray());

    public Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        if (_configuration.Value.TryGetValue(path, out JsonNode? value) && value is not null)
        {
            return Task.FromResult(value.Copy()!);
        }

        throw new VariableNotFoundException(path);
    }

    public Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
        => paths.ResolveMany(ResolveAsync, cancellationToken);

    public Task<string> SetAsync(string path, JsonNode value, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private static Dictionary<string, JsonNode?> ParseConfiguration(LocalVariableProviderConfiguration config)
    {
        using FileStream fileStream = File.OpenRead(config.FilePath);
        JsonNode node = JsonNode.Parse(fileStream) ?? throw new JsonException("Invalid Json Node");
        return JsonParser.ParseNode(node);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
