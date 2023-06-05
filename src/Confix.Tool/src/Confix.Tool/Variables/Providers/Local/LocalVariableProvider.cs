using System.Text.Json;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public sealed class LocalVariableProvider : IVariableProvider
{
    private readonly Lazy<Dictionary<string, string?>> _configuration;

    public LocalVariableProvider(JsonNode configuration)
        : this(LocalVariableProviderConfiguration.Parse(configuration))
    { }

    public LocalVariableProvider(LocalVariableProviderConfiguration configuration)
    {
        _configuration = new(() => ParseConfiguration(configuration));
    }

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<string>>(_configuration.Value.Keys.ToArray());

    public Task<string> ResolveAsync(string path, CancellationToken cancellationToken)
        => Task.FromResult(_configuration.Value.GetValueOrDefault(path)
            ?? throw new VariableNotFoundException("Value could not be resolved"));

    public async Task<IReadOnlyDictionary<string, string>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
    {
        Dictionary<string, string> values = new();
        List<VariableNotFoundException> errors = new();

        foreach (string path in paths)
        {
            try
            {
                var resolvedValue = await ResolveAsync(path, cancellationToken);
                values.Add(path, resolvedValue);
            }
            catch (VariableNotFoundException ex)
            {
                errors.Add(ex);
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException(errors);
        }

        return values;
    }

    public Task<string> SetAsync(string path, string value, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private static Dictionary<string, string?> ParseConfiguration(LocalVariableProviderConfiguration config)
    {
        // TODO: figure out how this relative file paths can be resolved
        using FileStream fileStream = File.OpenRead(config.FilePath);
        JsonNode node = JsonNode.Parse(fileStream) ?? throw new JsonException("Invalid Json Node");
        return JsonParser.ParseNode(node);
    }
}
