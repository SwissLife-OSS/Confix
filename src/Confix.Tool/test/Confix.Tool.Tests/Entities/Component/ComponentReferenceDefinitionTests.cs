using Confix.Tool;
using Confix.Tool.Abstractions;
using FluentAssertions;

namespace ConfiX.Entities.Component.Configuration;

public class ComponentReferenceDefinitionTests
{
    [Fact]
    public void From_Should_Throw_When_ProviderIsNull()
    {
        // Arrange
        var configuration = new ComponentReferenceConfiguration(
            null,
            "ComponentName",
            "1.0.0",
            true,
            new List<string>());

        // Act
        var exception = Assert.Throws<ValidationException>(() => ComponentReferenceDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void From_Should_Throw_When_ComponentNameIsNull()
    {
        // Arrange
        var configuration = new ComponentReferenceConfiguration(
            "Provider",
            null,
            "1.0.0",
            true,
            new List<string>());

        // Act
        var exception = Assert.Throws<ValidationException>(() => ComponentReferenceDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void From_Should_Throw_When_VersionIsNull()
    {
        // Arrange
        var configuration = new ComponentReferenceConfiguration(
            "Provider",
            "ComponentName",
            null,
            true,
            new List<string>());

        // Act
        var exception = Assert.Throws<ValidationException>(() => ComponentReferenceDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

     [Fact]
    public void From_Should_Throw_When_Name_And_ComponentName_And_Version()
    {
        // Arrange
        var configuration = new ComponentReferenceConfiguration(
            null,
            null,
            null,
            true,
            new List<string>());

        // Act
        var exception = Assert.Throws<ValidationException>(() => ComponentReferenceDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void From_Should_Return_ComponentReferenceDefinition()
    {
        // Arrange
        var configuration = new ComponentReferenceConfiguration(
            "Provider",
            "ComponentName",
            "1.0.0",
            true,
            new List<string>());

        // Act
        var definition = ComponentReferenceDefinition.From(configuration);

        // Assert
        definition.Provider.Should().Be(configuration.Provider);
        definition.ComponentName.Should().Be(configuration.ComponentName);
        definition.Version.Should().Be(configuration.Version);
        definition.IsEnabled.Should().Be(configuration.IsEnabled);
        definition.MountingPoints.Should().BeEquivalentTo(configuration.MountingPoints);
    }
}