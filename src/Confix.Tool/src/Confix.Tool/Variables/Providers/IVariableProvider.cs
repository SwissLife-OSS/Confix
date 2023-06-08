using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public interface IVariableProvider
{
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken);
    
    Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken);
    
    Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken);

    Task<string> SetAsync(string path, JsonNode value, CancellationToken cancellationToken);
}
