using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace Confix.Entities.Component.Configuration;

public class EnvironmentConfigurationTest : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "name": "dev",
                "enabled": true
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_OnlyName()
    {
        ExpectValid("\"dev\"");
    }

    [Fact]
    public void Parse_Should_BeValid_WhenEmpty()
    {
        ExpectValid(""" { } """);
    }

    public override object Parse(JsonNode json)
        => EnvironmentConfiguration.Parse(json);
}
