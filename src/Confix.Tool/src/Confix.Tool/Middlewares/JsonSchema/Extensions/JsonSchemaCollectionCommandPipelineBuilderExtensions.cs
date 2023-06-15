using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.JsonSchemas;

public static class JsonSchemaCollectionCommandPipelineBuilderExtensions
{
    public static IPipelineDescriptor UseJsonSchemaCollection(this IPipelineDescriptor builder)
        => builder.Use<JsonSchemaCollectionMiddleware>();
}
