using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.JsonSchemas;

public static class JsonSchemaCollectionCommandPipelineBuilderExtensions
{
    public static PipelineBuilder UseJsonSchemaCollection(this PipelineBuilder builder)
        => builder.Use<JsonSchemaCollectionMiddleware>();
}
