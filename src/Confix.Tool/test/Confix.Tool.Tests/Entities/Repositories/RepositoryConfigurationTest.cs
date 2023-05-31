using System.Text.Json.Nodes;
using Confix.Tool.Abstractions.Configuration;

namespace ConfiX.Entities.Component.Configuration;

public class RepositoryConfigurationTest : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "project": {
                    "environments": [
                        {
                            "name": "development",
                            "excludeFiles": ["appsettings.staging.json"]
                        },
                        "staging"
                    ]
                },
                "component": {
                   "name": "TestComponent"
                }
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

    public override object Parse(JsonNode json)
    {
        return RepositoryConfiguration.Parse(json);
    }
}