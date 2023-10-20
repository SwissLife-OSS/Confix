using System.Text.Json.Nodes;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Encryption;
using FluentAssertions;
using Moq;

namespace Confix.Tool.Tests.Middlewares.Encryption;

public class OptionalEncryptionMiddlewareTests
{
    [Fact]
    public void InvokeAsync_EncryptionConfigurationNull_DoesNotSetEncryptionFeature()
    {
        // Arrange
        var contextMock = new Mock<IMiddlewareContext>(MockBehavior.Strict);

        var featureCollection = new FeatureCollection();
        ConfigurationFeature configurationFeature = new(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            null,
            null,
            null,
            null,
            null
        );
        featureCollection.Set(configurationFeature);
        contextMock.SetupGet(x => x.Features).Returns(featureCollection);

        var encryptionProviderFactory = new Mock<IEncryptionProviderFactory>(MockBehavior.Strict);
        var middleware = new OptionalEncryptionMiddleware(encryptionProviderFactory.Object);
        bool isNextInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isNextInvoked = true;
            return Task.CompletedTask;
        };

        // Act
        middleware.InvokeAsync(contextMock.Object, next).Wait();

        // Assert
        featureCollection.TryGet<EncryptionFeature>(out _).Should().BeFalse();
        isNextInvoked.Should().BeTrue();
    }

    [Fact]
    public void InvokeAsync_EncryptionConfigurationSet_SetsEncryptionFeature()
    {
        // Arrange
        var featureCollection = new FeatureCollection();
        ConfigurationFeature configurationFeature = new(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            null,
            null,
            null,
            new(new("testProvider", new Dictionary<string, JsonObject>(), new())),
            null);
        featureCollection.Set(configurationFeature);

        var environmentFeature = new EnvironmentFeature(new("test", true));
        featureCollection.Set(environmentFeature);

        var contextMock = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        contextMock.SetupGet(x => x.Features).Returns(featureCollection);

        var encryptionProviderFactory = new Mock<IEncryptionProviderFactory>(MockBehavior.Strict);
        encryptionProviderFactory
            .Setup(x => x.CreateProvider(It.IsAny<EncryptionProviderConfiguration>()))
            .Returns(Mock.Of<IEncryptionProvider>());

        var middleware = new OptionalEncryptionMiddleware(encryptionProviderFactory.Object);
        bool isNextInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isNextInvoked = true;
            return Task.CompletedTask;
        };

        // Act
        middleware.InvokeAsync(contextMock.Object, next).Wait();

        // Assert
        featureCollection.TryGet<EncryptionFeature>(out _).Should().BeTrue();
        isNextInvoked.Should().BeTrue();
    }
}
