using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public class LocalVariableProvider : IVariableProvider
{
    public static readonly string PropertyType = "local";
    public LocalVariableProvider(JsonNode configuration)
    {
        _configuration = LocalVariableProviderConfiguration.Parse(configuration);
    }

    private readonly LocalVariableProviderConfiguration _configuration;

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyDictionary<string, string>> ResolveManyAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> SetAsync(string path, string value, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

