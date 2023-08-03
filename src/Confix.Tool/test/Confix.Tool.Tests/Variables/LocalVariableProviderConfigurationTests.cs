using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class LocalVariableProviderConfigurationTests
{
    [Fact]
    public void Parse_ValidJsonNode_CorrectResult()
    {
        // Arrange
        var jsonNode = JsonNode.Parse("""
            {
                "path": "/path/to/file.json"
            }
            """)!;

        // Act
        var config = LocalVariableProviderConfiguration.Parse(jsonNode);

        // Assert
        config.Path.Should().Be("/path/to/file.json");
    }

    [Fact]
    public void Parse_InvalidJsonNode_MissingProperty_SetsNull()
    {
        // Arrange
        var jsonNode = JsonNode.Parse("""
            {
                "some": "property"
            }
            """)!;

        // Act
        var config = LocalVariableProviderConfiguration.Parse(jsonNode);

        // Assert
        config.Path.Should().BeNull();
    }
}