namespace Confix.Tool.Common.Pipelines;

public static class MiddlewareContextExtensions
{
    public static void SetStatus(this IMiddlewareContext context, string message)
    {
        context.Status.Status = message;
    }

    public static MiddlewareContext WithExecutingDirectory(
        this IMiddlewareContext context,
        DirectoryInfo project)

    {
        return (MiddlewareContext) context with
        {
            Execution = (ExecutionContext) context.Execution with
            {
                CurrentDirectory = project
            }
        };
    }

    public static MiddlewareContext WithFeatureCollection(
        this IMiddlewareContext context,
        IFeatureCollection? featureCollection = null)

    {
        featureCollection ??= new FeatureCollection();
        return (MiddlewareContext) context with
        {
            Features = featureCollection
        };
    }
}
