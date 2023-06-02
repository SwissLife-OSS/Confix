using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace ConfiX.Entities.Component.Configuration;

public class ComponentRepositoriesConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "name": "common-components",
                "type": "git",
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
        => ComponentRepositoryConfiguration.Parse(json);
}
