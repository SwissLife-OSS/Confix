using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Entities.Component.Configuration;

namespace Confix.Tool.Tests.Entities.Encryption;

public class EncryptionProviderConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "type": "test"
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeValid_WhenEmpty()
    {
        ExpectValid(""" { } """);
    }

    [Fact]
    public void Parse_Should_BeValid_WithEnvironmentOverrides()
    {
        ExpectValid(
            """
            {
                "type": "test",
                "environmentOverride": {
                    "test": {
                        "type": "test"
                    }
                }
            }
            """);
    }

    public override object Parse(JsonNode json)
        => EncryptionProviderConfiguration.Parse(json);
}
