using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public class LocalVariableProvider : IVariableProvider
{
    public static readonly string PropertyType = "local";
    public LocalVariableProvider(JsonNode configuration)
    {
        var providerConfig = LocalVariableProviderConfiguration.Parse(configuration);
        _configuration = new(() => ParseConfiguration(providerConfig));
    }

    private readonly Lazy<Dictionary<string, string?>> _configuration;

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<string>>(_configuration.Value.Keys.ToArray());

    public Task<string> ResolveAsync(string path, CancellationToken cancellationToken)
        => Task.FromResult(_configuration.Value.GetValueOrDefault(path)
            ?? throw new Exception("Value could not be resolved"));

    public async Task<IReadOnlyDictionary<string, string>> ResolveManyAsync(
        IReadOnlyList<string> paths, 
        CancellationToken cancellationToken)
    {
        Dictionary<string, string> values = new();
        List<Exception> errors = new();

        foreach (string path in paths)
        {
            try
            {
                values.Add(path, await ResolveAsync(path, cancellationToken));
            }
            catch (Exception ex)
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
        using FileStream fileStream = File.OpenRead(config.FilePath);
        JsonNode node = JsonNode.Parse(fileStream) ?? throw new ArgumentException("Invalid Json Node");
        return JsonParser.ParseNode(node);
    }
}
