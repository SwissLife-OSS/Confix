using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Commands.Logging;

public static class OutputMiddlewareContextExtensions
{
    public static void SetOutput(this IMiddlewareContext context, object? result)
    {
        if (result is null)
        {
            return;
        }

        context.ContextData.Set(Context.Output, result);
    }
}
