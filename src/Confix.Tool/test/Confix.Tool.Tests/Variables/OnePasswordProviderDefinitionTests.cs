using Confix.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class OnePasswordProviderDefinitionTests
{
    [Fact]
    public void From_Should_ThrowValidationException_When_VaultMissing()
    {
        // arrange
        var configuration = new OnePasswordProviderConfiguration(null, null);

        // act & assert
        var exception = Assert.Throws<ValidationException>(()
            => OnePasswordProviderDefinition.From(configuration));

        exception.Errors.Should().Contain(e => e.Contains("Vault"));
    }

    [Fact]
    public void From_Should_DefaultServiceAccountToken()
    {
        // arrange
        var configuration = new OnePasswordProviderConfiguration("MyVault", null);

        // act
        var definition = OnePasswordProviderDefinition.From(configuration);

        // assert
        definition.Vault.Should().Be("MyVault");
        definition.ServiceAccountToken.Should().Be("$OP_SERVICE_ACCOUNT_TOKEN");
    }

    [Fact]
    public void From_Should_PreserveServiceAccountToken_When_Provided()
    {
        // arrange
        var configuration = new OnePasswordProviderConfiguration("MyVault", "$CUSTOM_TOKEN");

        // act
        var definition = OnePasswordProviderDefinition.From(configuration);

        // assert
        definition.Vault.Should().Be("MyVault");
        definition.ServiceAccountToken.Should().Be("$CUSTOM_TOKEN");
    }
}
