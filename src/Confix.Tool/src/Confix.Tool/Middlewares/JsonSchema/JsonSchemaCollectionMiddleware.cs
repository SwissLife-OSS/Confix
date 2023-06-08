using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.JsonSchemas;

public class JsonSchemaCollectionMiddleware : IMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.Features.Set(new JsonSchemaFeature());
        return next(context);
    }
}
