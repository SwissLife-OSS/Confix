using System.Text.Json.Nodes;
using ConfiX.Extensions;

namespace ConfiX.Entities.Component.Configuration;

public class ConfixConfigurationTest : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "project": {
                    "name": "ConfiX"
                },
                "component": {
                   "name": "TestComponent"
                },
                "isRoot": true
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_NonRequiredArMissing()
    {
        ExpectValid("{}");
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_ProjectIsNotObject()
    {
        ExpectInvalid(
            """
            {
                "project": "development"
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_ComponentNotObject()
    {
        ExpectInvalid(
            """
        {
            "component": "component"
        }
        """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_IsRootIsNotBoolean()
    {
        ExpectInvalid(
            """
        {
            "isRoot": "true"
        }
        """);
    }

    public override object Parse(JsonNode json)
    {
        return ConfixConfiguration.Parse(json);
    }
}
