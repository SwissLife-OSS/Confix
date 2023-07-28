using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

internal static class VariableProviderExtensions
{
    public static async Task<IReadOnlyDictionary<string, JsonNode>> ResolveMany(
        this IEnumerable<string> paths,
        Func<string, CancellationToken, Task<JsonNode>> resolveAsync,
        CancellationToken cancellationToken)
    {
        var errors = new ConcurrentQueue<VariableNotFoundException>();
        var resolvedVariables = new ConcurrentDictionary<string, JsonNode>();

        var parallelOptions = new ParallelOptions { CancellationToken = cancellationToken };

        async ValueTask ForEachAsync(string path, CancellationToken ctx)
        {
            try
            {
                var resolvedValue = await resolveAsync(path, ctx);
                resolvedVariables[path] = resolvedValue;
            }
            catch (VariableNotFoundException ex)
            {
                errors.Enqueue(ex);
            }
        }

        await Parallel.ForEachAsync(paths, parallelOptions, ForEachAsync);

        if (!errors.IsEmpty)
        {
            throw new AggregateException(errors);
        }

        return resolvedVariables;
    }
}
