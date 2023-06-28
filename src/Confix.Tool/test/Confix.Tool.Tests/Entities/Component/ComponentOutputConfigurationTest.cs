using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace Confix.Entities.Component.Configuration;

public class ComponentOutputConfigurationTest : ParserTestBase
{
    [Fact]
    public void Parse_Should_ReturnValidComponentOutputConfiguration_When_ValidJsonNodeProvided()
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
        => ComponentOutputConfiguration.Parse(json);
}
