using Moq;
using Azure.Security.KeyVault.Secrets;
using Confix.Variables;
using FluentAssertions;
using Azure;
using System.Text.Json.Nodes;

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
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act
        var result = await provider.ListAsync(context);

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
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act
        var result = await provider.ResolveAsync("foo", context);

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
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act
        var result = await provider.SetAsync("foo", JsonValue.Create("bar")!, context);

        // assert
        result.Should().Be("vault.key");
    }
}
