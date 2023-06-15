using System.CommandLine.Builder;

namespace Confix.Tool.Middlewares.JsonSchemas;

public static class MiddlewareCommandLineBuilderExtensions
{
    public static CommandLineBuilder RegisterJsonSchemaCollectionMiddleware(
        this CommandLineBuilder builder)
    {
        builder.AddTransient<JsonSchemaCollectionMiddleware>();

        return builder;
    }
}
