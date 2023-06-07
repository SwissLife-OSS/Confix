using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;

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
        result["foo"].Should().Be("1");
        result["bar"].Should().Be("baz");
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
        result["bar"].Should().Be("1");
        result["foo.foo"].Should().Be("1");
        result["foo.bar"].Should().Be("baz");
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
        result["[0]"].Should().Be("banana");
        result["[1].foo"].Should().Be("42");
        result["[1].bar"].Should().Be("baz");
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
        result["foo.[0].bar"].Should().Be("42");
        result["foo.[0].foo.[0].bar.baz"].Should().Be("420");
    }
}
