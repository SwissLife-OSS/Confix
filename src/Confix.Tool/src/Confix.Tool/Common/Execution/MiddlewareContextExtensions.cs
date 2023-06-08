namespace Confix.Tool.Common.Pipelines;

public static class MiddlewareContextExtensions
{
    public static void SetStatus(this IMiddlewareContext context, string message)
    {
        context.Status.Status = message;
    }
}
