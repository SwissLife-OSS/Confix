using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

internal static class VariableProviderExtensions
{
    public static async Task<IReadOnlyDictionary<string, JsonNode>> ResolveMany(
        this IEnumerable<string> paths,
        Func<string, CancellationToken, Task<JsonNode>> resolveAsync,
        CancellationToken cancellationToken)
    {
        ConcurrentQueue<VariableNotFoundException> errors = new();
        ConcurrentQueue<KeyValuePair<string, JsonNode>> resolvedVariables = new();

        await Parallel.ForEachAsync(
            paths.Distinct(),
            new ParallelOptions { CancellationToken = cancellationToken },
            async (path, ctx) =>
            {
                try
                {
                    JsonNode resolvedValue = await resolveAsync(path, ctx);
                    resolvedVariables.Enqueue(new(path, resolvedValue));
                }
                catch (VariableNotFoundException ex)
                {
                    errors.Enqueue(ex);
                }
            });

        if (!errors.IsEmpty)
        {
            throw new AggregateException(errors);
        }

        return new Dictionary<string, JsonNode>(resolvedVariables);
    }
}
