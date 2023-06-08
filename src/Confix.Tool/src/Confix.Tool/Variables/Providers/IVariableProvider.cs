using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public interface IVariableProvider
{
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken);
    
    Task<JsonValue> ResolveAsync(string path, CancellationToken cancellationToken);
    
    Task<IReadOnlyDictionary<string, JsonValue>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken);

    Task<string> SetAsync(string path, JsonValue value, CancellationToken cancellationToken);
}
