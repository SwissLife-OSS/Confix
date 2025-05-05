using System.Text.Json.Nodes;
using Confix.Tool;

namespace Confix.Variables;

public static class VariableResolverExtensions
{
    public static async Task<JsonNode> ResolveOrThrowAsync(
        this IVariableResolver resolver,
        VariablePath path,
        IVariableProviderContext context)
    {
        try
        {
            return await resolver.ResolveVariable(path, context);
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
        IVariableProviderContext context)
    {
        try
        {
            return await service.RewriteAsync(node, context);
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
