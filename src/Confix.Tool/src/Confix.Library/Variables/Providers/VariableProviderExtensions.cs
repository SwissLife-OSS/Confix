using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace Confix.Variables;

internal static class VariableProviderExtensions
{
    public static async Task<IReadOnlyDictionary<string, JsonNode>> ResolveMany(
        this IEnumerable<string> paths,
        Func<string, IVariableProviderContext, Task<JsonNode>> resolveAsync,
        IVariableProviderContext context)
    {
        var errors = new ConcurrentQueue<VariableNotFoundException>();
        var resolvedVariables = new ConcurrentDictionary<string, JsonNode>();

        var parallelOptions = new ParallelOptions { CancellationToken = context.CancellationToken };

        async ValueTask ForEachAsync(string path, IVariableProviderContext ctx)
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

        await Parallel.ForEachAsync(paths, parallelOptions, (path, _) => ForEachAsync(path, context));

        if (!errors.IsEmpty)
        {
            throw new AggregateException(errors);
        }

        return resolvedVariables;
    }
}
