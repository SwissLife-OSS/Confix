using System.Text.Json.Nodes;

namespace Confix.Variables;

public interface IVariableProvider : IAsyncDisposable
{
    public static virtual string Type => throw new NotImplementedException();

    /// <summary>
    /// Gets all available variables
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of variable paths available on this provider</returns>
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the value of the variable at the given path.
    /// </summary>
    /// <param name="path">The path to the variable.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value of the variable.</returns>
    Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sets the value of the variable at the given path.
    /// </summary>
    /// <param name="path">The path to the variable.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The path of the variable.</returns>
    Task<string> SetAsync(string path, JsonNode value, CancellationToken ct);
}
