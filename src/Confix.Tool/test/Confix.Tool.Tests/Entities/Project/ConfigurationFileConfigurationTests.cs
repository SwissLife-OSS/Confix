using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace ConfiX.Entities.Component.Configuration;

public class ConfigurationFileConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "type": "dotnet-appsettings",
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
    public void Parse_Should_BeValid_When_JustAString()
    {
        ExpectValid("\"inline\"");
    }

    public override object Parse(JsonNode json)
    {
        return ConfigurationFileConfiguration.Parse(json);
    }
}
