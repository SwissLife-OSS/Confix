using Confix.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class OnePasswordProviderDefinitionTests
{
    [Fact]
    public void From_Should_ThrowValidationException_When_VaultMissing()
    {
        // arrange
        var configuration = new OnePasswordProviderConfiguration(null, null, null);

        // act & assert
        var exception = Assert.Throws<ValidationException>(()
            => OnePasswordProviderDefinition.From(configuration));

        exception.Errors.Should().Contain(e => e.Contains("Vault"));
    }

    [Fact]
    public void From_Should_LeaveServiceAccountTokenNull_When_NotProvided()
    {
        // arrange
        var configuration = new OnePasswordProviderConfiguration("MyVault", null, null);

        // act
        var definition = OnePasswordProviderDefinition.From(configuration);

        // assert
        definition.Vault.Should().Be("MyVault");
        definition.ServiceAccountToken.Should().BeNull();
    }

    [Fact]
    public void From_Should_PreserveServiceAccountToken_When_Provided()
    {
        // arrange
        var configuration = new OnePasswordProviderConfiguration("MyVault", "$CUSTOM_TOKEN", null);

        // act
        var definition = OnePasswordProviderDefinition.From(configuration);

        // assert
        definition.Vault.Should().Be("MyVault");
        definition.ServiceAccountToken.Should().Be("$CUSTOM_TOKEN");
    }
}
