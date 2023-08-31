using System.Text.Json.Nodes;
using Confix.Tool;

namespace Confix.Variables;

public static class VariableResolverExtensions
{
    public static async Task<JsonNode> ResolveOrThrowAsync(
        this IVariableResolver resolver,
        VariablePath path,
        CancellationToken cancellationToken)
    {
        try
        {
            return await resolver.ResolveVariable(path, cancellationToken);
        }
        catch (AggregateException ex)
        {
            var names = ex.InnerExceptions
                .OfType<VariableNotFoundException>()
                .Select(x => x.Path)
                .ToArray();

            if (names.Length > 0)
            {
                throw ThrowHelper.VariablesNotFound(names);
            }

            throw;
        }
        catch (VariableNotFoundException)
        {
            throw ThrowHelper.VariableNotFound(path.ToString());
        }
    }

    public static async Task<JsonNode?> RewriteOrThrowAsync(
        this IVariableReplacerService service,
        JsonNode? node,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.RewriteAsync(node, cancellationToken);
        }
        catch (AggregateException ex)
        {
            var names = ex.InnerExceptions
                .OfType<VariableNotFoundException>()
                .Select(x => x.Path)
                .ToArray();

            if (names.Length > 0)
            {
                throw ThrowHelper.VariablesNotFound(names);
            }

            throw;
        }
        catch (VariableNotFoundException ex)
        {
            throw ThrowHelper.VariableNotFound(ex.Path);
        }
    }
}
