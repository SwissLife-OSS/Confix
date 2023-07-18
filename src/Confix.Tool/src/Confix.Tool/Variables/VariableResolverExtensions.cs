using System.Text.Json.Nodes;
using Confix.Tool;

namespace ConfiX.Variables;

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
        catch (VariableNotFoundException)
        {
            throw new ExitException($"Variable not found: {path}")
            {
                Help = "Run [blue]confix variable list[/] to see all available variables."
            };
        }
    }
}
