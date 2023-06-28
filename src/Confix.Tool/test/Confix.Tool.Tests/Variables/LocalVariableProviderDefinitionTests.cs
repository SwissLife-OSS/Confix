using ConfiX.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class LocalVariableProviderDefinitionTests{
    [Fact]
    public void From_ValidConfiguration_CorrectResult()
    {
        // Arrange
        var configuration = new LocalVariableProviderConfiguration("/path/to/file.json");

        // Act
        var definition = LocalVariableProviderDefinition.From(configuration);

        // Assert
        definition.Path.Should().Be("/path/to/file.json");
    }

    [Fact]
    public void From_InvalidConfiguration_MissingPath_ThrowsValidationException()
    {
        // Arrange
        var configuration = new LocalVariableProviderConfiguration(null);

        // Act
        var exception = Assert.Throws<ValidationException>(
            () => LocalVariableProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().Contain("Path is required");
    }
}