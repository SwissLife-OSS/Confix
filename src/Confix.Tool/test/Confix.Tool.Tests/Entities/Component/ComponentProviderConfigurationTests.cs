using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace ConfiX.Entities.Component.Configuration;

public class ComponentProviderConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "name": "dotnet",
                "type": "dotnet-package",
                "additional": "property"
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_WhenEmpty()
    {
        ExpectValid(""" { } """);
    }

    /// <inheritdoc />
    public override object Parse(JsonNode json)
        => ComponentProviderConfiguration.Parse(json);
}
