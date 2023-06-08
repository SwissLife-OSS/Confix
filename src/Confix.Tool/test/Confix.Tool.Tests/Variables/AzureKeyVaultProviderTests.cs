using Moq;
using Azure.Security.KeyVault.Secrets;
using ConfiX.Variables;
using FluentAssertions;
using Azure;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Confix.Tool.Tests;

public class AzureKeyVaultProviderTests
{
    [Fact]
    public async Task ListAsync_Should_ReturnKeysAsync()
    {
        // arrange
        Mock<SecretClient> secretClientMock = new();
        Mock<AsyncPageable<SecretProperties>> pageableMock = new();
        Mock<IAsyncEnumerator<SecretProperties>> enumerableMock = new();

        enumerableMock
            .SetupSequence(x => x.MoveNextAsync())
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        enumerableMock
            .SetupSequence(x => x.Current)
            .Returns(new SecretProperties("foo"))
            .Returns(new SecretProperties("bar"));

        pageableMock
            .Setup(x => x.GetAsyncEnumerator(default))
            .Returns(enumerableMock.Object);

        secretClientMock
            .Setup(x => x.GetPropertiesOfSecretsAsync(default))
            .Returns(pageableMock.Object);

        AzureKeyVaultProvider provider = new(secretClientMock.Object);
        // act
        var result = await provider.ListAsync(default);

        // assert
        result.Should().HaveCount(2);
        result.Should().Contain("foo");
        result.Should().Contain("bar");
    }

    [Fact]
    public async Task ResolveAsync_Should_ResolveVariableAsync()
    {
        // arrange
        Mock<SecretClient> secretClientMock = new();
        Mock<Response<KeyVaultSecret>> responseMock = new();
        responseMock
            .SetupGet(x => x.Value)
            .Returns(new KeyVaultSecret("foo", "bar"));

        secretClientMock
            .Setup(x => x.GetSecretAsync("foo", null, default))
            .ReturnsAsync(responseMock.Object);

        AzureKeyVaultProvider provider = new(secretClientMock.Object);

        // act
        var result = await provider.ResolveAsync("foo", default);

        // assert
        ((string?)result)?.Should().Be("bar");
    }

    [Fact]
    public async Task SetAsync_Should_SetAndReturn()
    {
        // arrange
        Mock<SecretClient> secretClientMock = new();
        Mock<Response<KeyVaultSecret>> responseMock = new();
        responseMock
            .SetupGet(x => x.Value)
            .Returns(new KeyVaultSecret("vault-key", "bar"));

        secretClientMock
            .Setup(x => x.SetSecretAsync("foo", "bar", default))
            .ReturnsAsync(responseMock.Object);
        AzureKeyVaultProvider provider = new(secretClientMock.Object);

        // act
        var result = await provider.SetAsync("foo", JsonValue.Create("bar"), default);

        // assert
        result.Should().Be("vault-key");
    }
}
