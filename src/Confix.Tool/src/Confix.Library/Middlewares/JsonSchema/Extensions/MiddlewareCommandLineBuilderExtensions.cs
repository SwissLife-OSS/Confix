
namespace Confix.Tool.Middlewares.JsonSchemas;

public static class MiddlewareCommandLineBuilderExtensions
{
    public static ConfixCommandLineBuilder RegisterJsonSchemaCollectionMiddleware(
        this ConfixCommandLineBuilder builder)
    {
        builder.AddTransient<JsonSchemaCollectionMiddleware>();

        return builder;
    }
}
