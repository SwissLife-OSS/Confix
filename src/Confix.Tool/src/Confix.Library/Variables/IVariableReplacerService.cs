using System.Text.Json.Nodes;

namespace Confix.Variables;

public interface IVariableReplacerService
{
    Task<JsonNode?> RewriteAsync(JsonNode? node, IVariableProviderContext context);
}
