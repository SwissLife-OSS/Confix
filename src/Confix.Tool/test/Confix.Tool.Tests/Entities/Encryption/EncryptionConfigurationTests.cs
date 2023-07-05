using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using ConfiX.Entities.Component.Configuration;

namespace Confix.Tool.Tests.Entities.Encryption;

public class EncryptionConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "provider": {
                    "type": "test"
                }
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeValid_WhenEmpty()
    {
        ExpectValid(""" { } """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_ProviderIsNotAnObject()
    {
        ExpectInvalid(
            """
            {
                "provider": "test"
            }
            """);
    }


    public override object Parse(JsonNode json)
       => EncryptionConfiguration.Parse(json);
}