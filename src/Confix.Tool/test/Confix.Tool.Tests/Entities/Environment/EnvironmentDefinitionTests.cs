using Confix.Tool;
using Confix.Tool.Abstractions;
using FluentAssertions;

namespace ConfiX.Entities.Component.Configuration;

public class EnvironmentDefinitionTests
{
    [Fact]
    public void From_Should_BeValid()
    {
        // Arrange
        var configuration = new EnvironmentConfiguration("dev", true);

        // Act
        var definition = EnvironmentDefinition.From(configuration);

        // Assert
        Assert.Equal("dev", definition.Name);
        Assert.True(definition.Enabled);
    }

    [Fact]
    public void From_Should_BeInvalid_When_NameIsNull()
    {
        // Arrange
        var configuration = new EnvironmentConfiguration(null, true);

         // Act
        var exception = Assert.Throws<ValidationException>(() => EnvironmentDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void From_Should_BeInvalid_When_NameIsEmpty()
    {
        // Arrange
        var configuration = new EnvironmentConfiguration(string.Empty, true);

         // Act
        var exception = Assert.Throws<ValidationException>(() => EnvironmentDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void From_Should_BeValid_When_EnabledIsNull()
    {
        // Arrange
        var configuration = new EnvironmentConfiguration("dev", null);

        // Act
        var definition = EnvironmentDefinition.From(configuration);

        // Assert
        Assert.Equal("dev", definition.Name);
        Assert.False(definition.Enabled);
    }
}