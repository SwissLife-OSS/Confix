using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Abstractions;
using FluentAssertions;

namespace ConfiX.Entities.Component.Configuration;

public class ComponentProviderDefinitionTests
{
    [Fact]
    public void From_Should_Throw_When_NameIsNull()
    {
        // Arrange
        var configuration = new ComponentProviderConfiguration(
            null,
            "ProviderType",
            new JsonObject());

        // Act
        var exception = Assert.Throws<ValidationException>(() => ComponentProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void From_Should_Throw_When_TypeIsNull()
    {
        // Arrange
        var configuration = new ComponentProviderConfiguration(
            "TestProvider",
            null,
            new JsonObject());

        // Act
        var exception = Assert.Throws<ValidationException>(() => ComponentProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void From_Should_Throw_When_TypeAndNameIsNull(){
        // Arrange
        var configuration = new ComponentProviderConfiguration(
            null,
            null,
            new JsonObject());

        // Act
        var exception = Assert.Throws<ValidationException>(() => ComponentProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void From_Should_ReturnDefinition_When_NameAndTypeAreNotNull()
    {
        // Arrange
        var configuration = new ComponentProviderConfiguration(
            "TestProvider",
            "ProviderType",
            new JsonObject());

        // Act
        var definition = ComponentProviderDefinition.From(configuration);

        // Assert
        Assert.Equal("TestProvider", definition.Name);
        Assert.Equal("ProviderType", definition.Type);
        Assert.Empty(definition.Value);
    }
}