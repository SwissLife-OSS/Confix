using System.Text.Json.Nodes;

namespace Confix.Variables;

public interface IVariableProvider : IAsyncDisposable
{
    public static virtual string Type => throw new NotImplementedException();

    /// <summary>
    /// Gets all available variables
    /// </summary>
    /// <returns>A list of variable paths available on this provider</returns>
    Task<IReadOnlyList<string>> ListAsync(IVariableProviderContext context);

    /// <summary>
    /// Gets the value of the variable at the given path.
    /// </summary>
    /// <returns>The value of the variable.</returns>
    Task<JsonNode> ResolveAsync(string path, IVariableProviderContext context);

    Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        IVariableProviderContext context);

    /// <summary>
    /// Sets the value of the variable at the given path.
    /// </summary>
    /// <returns>The path of the variable.</returns>
    Task<string> SetAsync(string path, JsonNode value, IVariableProviderContext context);
}
