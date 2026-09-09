using System.Text.Json.Nodes;
using Confix.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class OnePasswordProviderConfigurationTests
{
    [Fact]
    public void Parse_Should_DeserializeAllFields()
    {
        // arrange
        var json = JsonNode.Parse("""
        {
            "vault": "MyVault",
            "serviceAccountToken": "$MY_TOKEN"
        }
        """)!;

        // act
        var configuration = OnePasswordProviderConfiguration.Parse(json);

        // assert
        configuration.Should().NotBeNull();
        configuration.Vault.Should().Be("MyVault");
        configuration.ServiceAccountToken.Should().Be("$MY_TOKEN");
    }

    [Fact]
    public void Parse_Should_HandleMissingOptionalFields()
    {
        // arrange
        var json = JsonNode.Parse("""
        {
            "vault": "MyVault"
        }
        """)!;

        // act
        var configuration = OnePasswordProviderConfiguration.Parse(json);

        // assert
        configuration.Should().NotBeNull();
        configuration.Vault.Should().Be("MyVault");
        configuration.ServiceAccountToken.Should().BeNull();
    }
}
