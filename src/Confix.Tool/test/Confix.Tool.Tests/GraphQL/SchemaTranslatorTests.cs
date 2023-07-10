using System.Text.Json;
using Confix.Tool.Schema;
using Json.Schema;
using Snapshooter.Xunit;

namespace ConfiX.GraphQL;

public sealed class SchemaTranslatorTests
{
    [Fact]
    public void Scalar_DefaultValue_Should_BeTranslatedCorrectly()
    {
        // arrange
        string schema = """
            type Query {
                foo: String @defaultValue(value: "bar")
            }
        """;

        // act
        var result = SchemaHelpers.BuildSchema(schema).ToJsonSchema();

        // assert
        Match(result);
    }

    [Fact]
    public void Object_DefaultValue_Should_BeTranslatedCorrectly()
    {
        // arrange
        string schema = """
            type Query {
                foo: Foo @defaultValue(value: { bar: "baz" })
            }
            type Foo {
                bar: String
            }
        """;

        // act
        var result = SchemaHelpers.BuildSchema(schema).ToJsonSchema();

        // assert
        Match(result);
    }

    private static void Match(JsonSchemaBuilder builder) => Serialize(builder).MatchSnapshot();

    private static string Serialize(JsonSchemaBuilder builder)
        => JsonSerializer.Serialize(builder.Build(),
            new JsonSerializerOptions() { WriteIndented = true });
}
