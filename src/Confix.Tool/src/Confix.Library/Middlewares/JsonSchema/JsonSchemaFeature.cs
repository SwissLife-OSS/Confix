namespace Confix.Tool.Middlewares.JsonSchemas;

public sealed class JsonSchemaFeature
{
    public IList<JsonSchemaDefinition> Schemas { get; set; } = new List<JsonSchemaDefinition>();
}
