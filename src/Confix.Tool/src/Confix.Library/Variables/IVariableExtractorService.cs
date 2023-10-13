using System.Text.Json.Nodes;

namespace Confix.Variables;

public interface IVariableExtractorService
{
    Task<IEnumerable<VariableInfo>> ExtractAsync(
        JsonNode? node,
        CancellationToken cancellationToken);
}
