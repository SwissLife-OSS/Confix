using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public interface IVariableReplacerService
{
    Task<JsonNode?> RewriteAsync(JsonNode? node, CancellationToken cancellationToken);
}
