using System.Text.Json.Nodes;
using Confix.Variables;
using FluentAssertions;
using Moq;

namespace Confix.Tool.Tests;

public class OnePasswordProviderTests
{
    [Fact]
    public async Task ResolveAsync_Should_ReturnValue_When_SecretExists()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        cliMock
            .Setup(x => x.ReadAsync("TestVault", "db", "password", It.IsAny<CancellationToken>()))
            .ReturnsAsync("s3cret");

        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act
        var result = await provider.ResolveAsync("db.password", context);

        // assert
        ((string?)result).Should().Be("s3cret");
    }

    [Fact]
    public async Task ResolveAsync_Should_ThrowVariableNotFound_When_SecretMissing()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        cliMock
            .Setup(x => x.ReadAsync("TestVault", "db", "password", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OnePasswordCliException(1, "item not found"));

        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act & assert
        await Assert.ThrowsAsync<VariableNotFoundException>(()
            => provider.ResolveAsync("db.password", context));
    }

    [Fact]
    public async Task ListAsync_Should_ReturnTitlesAndIds()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        cliMock
            .Setup(x => x.ListItemsAsync("TestVault", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OnePasswordItemSummary>
            {
                new("id1", "db"),
                new("id2", "api")
            });

        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act
        var result = await provider.ListAsync(context);

        // assert
        result.Should().HaveCount(4);
        result.Should().Contain("db");
        result.Should().Contain("api");
        result.Should().Contain("id1");
        result.Should().Contain("id2");
    }

    [Fact]
    public async Task ResolveAsync_Should_DefaultFieldToPassword_When_PathHasNoDot()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        cliMock
            .Setup(x => x.ReadAsync("TestVault", "db", "password", It.IsAny<CancellationToken>()))
            .ReturnsAsync("s3cret");

        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act
        var result = await provider.ResolveAsync("db", context);

        // assert
        ((string?)result).Should().Be("s3cret");
    }

    [Fact]
    public async Task SetAsync_Should_EditExistingItem()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        cliMock
            .Setup(x => x.EditItemFieldAsync(
                "TestVault", "db", "password", "new-secret", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act
        var result = await provider.SetAsync("db.password", JsonValue.Create("new-secret")!, context);

        // assert
        result.Should().Be("db.password");
        cliMock.Verify(
            x => x.EditItemFieldAsync(
                "TestVault", "db", "password", "new-secret", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_Should_CreateItem_When_EditFails()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        cliMock
            .Setup(x => x.EditItemFieldAsync(
                "TestVault", "db", "password", "new-secret", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OnePasswordCliException(1, "not found"));

        cliMock
            .Setup(x => x.CreateItemAsync(
                "TestVault", "db", "password", "new-secret", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act
        var result = await provider.SetAsync("db.password", JsonValue.Create("new-secret")!, context);

        // assert
        result.Should().Be("db.password");
        cliMock.Verify(
            x => x.CreateItemAsync(
                "TestVault", "db", "password", "new-secret", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_Should_NotCreateItem_When_EditFailsWithNonNotFoundError()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        cliMock
            .Setup(x => x.EditItemFieldAsync(
                "TestVault", "db", "password", "new-secret", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OnePasswordCliException(1, "authentication failed"));

        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act & assert
        await Assert.ThrowsAsync<ExitException>(()
            => provider.SetAsync("db.password", JsonValue.Create("new-secret")!, context));

        cliMock.Verify(
            x => x.CreateItemAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAsync_Should_ThrowNotSupported_When_ValueNotString()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act & assert
        await Assert.ThrowsAsync<NotSupportedException>(()
            => provider.SetAsync("db.password", JsonValue.Create(42)!, context));
    }

    [Fact]
    public async Task ResolveAsync_Should_Throw_When_PathEndsWithDot()
    {
        // arrange
        Mock<IOnePasswordCli> cliMock = new();
        var definition = new OnePasswordProviderDefinition("TestVault", "$OP_SERVICE_ACCOUNT_TOKEN", null);
        var provider = new OnePasswordProvider(definition, cliMock.Object);
        var context = new VariableProviderContext(null, CancellationToken.None);

        // act & assert
        await Assert.ThrowsAsync<VariableNotFoundException>(()
            => provider.ResolveAsync("db.", context));
    }
}
