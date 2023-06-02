using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace ConfiX.Entities.Component.Configuration;

public class ComponentInputConfigurationTest : ParserTestBase
{
    [Fact]
    public void Parse_Should_ReturnValidComponentInputConfiguration_When_ValidJsonNodeProvided()
    {
        ExpectValid(
            """
            {
                "type": "graphql",
                "additional": "property"
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_WhenEmpty()
    {
        ExpectValid(""" { } """);
    }

    [Fact]
    public void Parse_ShouldBeValid_When_AdditionalIsMissing()
    {
        ExpectValid(
            """
            {
                "type": "graphql"
            }
         """);
    }

    public override object Parse(JsonNode json)
        => ComponentInputConfiguration.Parse(json);
}
