using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace ConfiX.Entities.Component.Configuration;

public class ComponentConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_ReturnValidComponentConfiguration_When_ValidJsonNodeProvided()
    {
        ExpectValid(
            """
            {
                "name": "TestComponent",
                "inputs": [
                    {
                        "type": "graphql",
                        "additional": "property"
                    },
                    {
                        "type": "dotnet",
                        "additional2": "property"
                    }
                ],
                "outputs": [
                    {
                        "type": "schema",
                        "additional2": "property"
                    }
                ]
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_WhenEmpty()
    {
        ExpectValid(""" { } """);
    }

    [Fact]
    public void Parse_ShouldBeInvalid_When_InputsIsNotAnArray()
    {
        ExpectInvalid(
            """
            {
                "name": "TestComponent",
                "inputs": {
                    "type": "graphql",
                    "additional": "property"
                },
                "outputs": [
                    {
                        "type": "schema",
                        "additional2": "property"
                    }
                ]
            }
         """);
    }

    [Fact]
    public void Parse_ShouldBeInvalid_When_OutputsIsNotAnArray()
    {
        ExpectInvalid(
            """
            {
                "name": "TestComponent",
                "inputs": [
                    {
                        "type": "graphql",
                        "additional": "property"
                    },
                    {
                        "type": "dotnet",
                        "additional2": "property"
                    }
                ],
                "outputs": {
                    "type": "schema",
                    "additional2": "property"
                }
            }
         """);
    }

    /// <inheritdoc />
    public override object Parse(JsonNode json)
        => ComponentConfiguration.Parse(json);
}
