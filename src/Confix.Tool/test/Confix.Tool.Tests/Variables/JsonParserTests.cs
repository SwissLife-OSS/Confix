using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;
using Json.More;

namespace Confix.Tool.Tests;

public class JsonParserTests
{
    [Theory]
    [InlineData("5")]
    [InlineData("5.5")]
    [InlineData("\"foo\"")]
    public void ParseNode_PrimitiveType_ThrowsException(string json)
    {
        // arrange
        Action action = () => JsonParser.ParseNode(JsonNode.Parse(json)!);

        // act & assert
        action.Should()
            .ThrowExactly<JsonParserException>()
            .WithMessage("Node must be an JsonObject or JsonArray");
    }

    [Fact]
    public void ParseNode_ObjectWithoutNesting_ReturnsCorrectResult()
    {
        // arrange
        JsonNode objectNode = JsonNode.Parse(
            """
            {
                "foo": 1,
                "bar": "baz",
                "banana": null
            }
            """)!;

        // act
        var result = JsonParser.ParseNode(objectNode);

        // act & assert
        result.Should().HaveCount(3);
        Assert.True(result["foo"].IsEquivalentTo(JsonValue.Create(1)));
        Assert.True(result["bar"].IsEquivalentTo(JsonValue.Create("baz")));
        result["banana"].Should().BeNull();
    }

    [Fact]
    public void ParseNode_ObjectWithNesting_ReturnsCorrectResult()
    {
        // arrange
        JsonNode objectNode = JsonNode.Parse(
            """
            {
                "bar": 1,
                "foo": {
                    "foo": 1,
                    "bar": "baz",
                    "banana": null
                }
            }
            """)!;

        // act
        var result = JsonParser.ParseNode(objectNode);

        // act & assert
        result.Should().HaveCount(4);
        Assert.True(result["bar"].IsEquivalentTo(JsonValue.Create(1)));
        Assert.True(result["foo.foo"].IsEquivalentTo(JsonValue.Create(1)));
        Assert.True(result["foo.bar"].IsEquivalentTo(JsonValue.Create("baz")));
        result["foo.banana"].Should().BeNull();
    }

    [Fact]
    public void ParseNode_ArrayInRoot_ReturnsCorrectResult()
    {
        // arrange
        JsonNode objectNode = JsonNode.Parse(
            """
            [
                "banana",
                {
                    "foo": 42,
                    "bar": "baz"
                },
                null
            ]
            """)!;

        // act
        var result = JsonParser.ParseNode(objectNode);

        // act & assert
        result.Should().HaveCount(4);
        Assert.True(result["[0]"].IsEquivalentTo(JsonValue.Create("banana")));
        Assert.True(result["[1].foo"].IsEquivalentTo(JsonValue.Create(42)));
        Assert.True(result["[1].bar"].IsEquivalentTo(JsonValue.Create("baz")));
        result["[2]"].Should().BeNull();
    }

    [Fact]
    public void ParseNode_NestedArray_ReturnsCorrectResult()
    {
        // arrange
        JsonNode objectNode = JsonNode.Parse(
            """
            {
                "foo": [
                    {
                        "bar": 42,
                        "foo": [
                            {
                                "bar.baz": 420
                            }
                        ]
                    }
                ]
            }
            """)!;

        // act
        var result = JsonParser.ParseNode(objectNode);

        // act & assert
        result.Should().HaveCount(2);
        Assert.True(result["foo.[0].bar"].IsEquivalentTo(JsonValue.Create(42)));
        Assert.True(result["foo.[0].foo.[0].bar.baz"].IsEquivalentTo(JsonValue.Create(420)));
    }
}
