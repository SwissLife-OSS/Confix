using System.Text.Json;
using Confix.Entities.Schema;
using Confix.Tool.Schema;
using Json.Schema;

namespace Confix.Tool.Tests.Entities.Schema;

public class MetadataKeywordTests
{
    private static readonly IKeywordHandler _handler = MetadataKeyword.Instance;

    [Fact]
    public void Name_IsMetadata()
    {
        // act & assert
        Assert.Equal("metadata", _handler.Name);
    }

    [Fact]
    public void ValidateKeywordValue_WithArray_ReturnsValue()
    {
        // arrange
        var value = JsonSerializer.Deserialize<JsonElement>("""["item1", "item2"]""");

        // act
        var result = _handler.ValidateKeywordValue(value);

        // assert
        var element = Assert.IsType<JsonElement>(result);
        Assert.Equal(JsonValueKind.Array, element.ValueKind);
        Assert.Equal(2, element.GetArrayLength());
    }

    [Fact]
    public void ValidateKeywordValue_WithNonArray_Throws()
    {
        // arrange
        var value = JsonSerializer.Deserialize<JsonElement>("""{"key": "value"}""");

        // act & assert
        Assert.Throws<JsonSchemaException>(() => _handler.ValidateKeywordValue(value));
    }

    [Fact]
    public void Evaluate_SchemaWithMetadata_ProducesAnnotation()
    {
        // arrange
        var schema = ConfixJsonSchema.FromText(
            """
            {
              "type": "object",
              "properties": {
                "name": {
                  "type": "string",
                  "metadata": [{ "type": "dependency", "kind": "graphql" }]
                }
              }
            }
            """);

        var instance = JsonSerializer.Deserialize<JsonElement>("""{"name": "confix"}""");

        var options = new EvaluationOptions
        {
            OutputFormat = Json.Schema.OutputFormat.List,
            PreserveDroppedAnnotations = true
        };

        // act
        var results = schema.Evaluate(instance, options);

        // assert
        Assert.True(results.IsValid);
        Assert.Contains(
            results.Details!,
            x => x.Annotations?.ContainsKey(MetadataKeyword.Name) is true);
    }

    [Fact]
    public void Build_SchemaWithInvalidMetadata_Throws()
    {
        // act & assert
        Assert.ThrowsAny<Exception>(() => ConfixJsonSchema.FromText(
            """
            {
              "type": "object",
              "metadata": "not-an-array"
            }
            """));
    }
}
