using System.CommandLine;
using Confix.Tool;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using FluentAssertions;
using Moq;

namespace ConfiX.Entities.Component.Configuration.Middlewares;

public class EnvironmentMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_EnabledEnvironment_ShouldSetFeature()
    {
        // arrange
        var testEnvironment = new EnvironmentDefinition("test", true);
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);

        var featureCollection = new FeatureCollection();
        ConfigurationFeature configurationFeature = new(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            new ProjectDefinition(
                "Test",
                new[] { testEnvironment },
                Array.Empty<ComponentReferenceDefinition>(),
                Array.Empty<ComponentRepositoryDefinition>(),
                Array.Empty<VariableProviderDefinition>(),
                Array.Empty<ComponentProviderDefinition>(),
                Array.Empty<ConfigurationFileDefinition>(),
                Array.Empty<ProjectDefinition>(),
                null
            ),
            null,
            null
        );
        featureCollection.Set(configurationFeature);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        middelwareContext.SetupGet(x => x.Parameter).Returns(ParameterCollection.Empty());
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);

        var middleware = new EnvironmentMiddleware();
        bool isNextInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isNextInvoked = true;
            return Task.CompletedTask;
        };

        // act
        await middleware.InvokeAsync(middelwareContext.Object, next);

        // assert
        isNextInvoked.Should().BeTrue();
        var environmentFeature = featureCollection.Get<EnvironmentFeature>();
        environmentFeature.Should().NotBeNull();
        environmentFeature.ActiveEnvironment.Should().Be(testEnvironment);
    }

    [Fact]
    public async Task InvokeAsync_NoEnvironment_Throws()
    {
        // arrange
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);

        var featureCollection = new FeatureCollection();
        ConfigurationFeature configurationFeature = new(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            new ProjectDefinition(
                "Test",
                Array.Empty<EnvironmentDefinition>(),
                Array.Empty<ComponentReferenceDefinition>(),
                Array.Empty<ComponentRepositoryDefinition>(),
                Array.Empty<VariableProviderDefinition>(),
                Array.Empty<ComponentProviderDefinition>(),
                Array.Empty<ConfigurationFileDefinition>(),
                Array.Empty<ProjectDefinition>(),
                null
            ),
            null,
            null
        );
        featureCollection.Set(configurationFeature);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        middelwareContext.SetupGet(x => x.Parameter).Returns(ParameterCollection.Empty());
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);

        var middleware = new EnvironmentMiddleware();
        bool isNextInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isNextInvoked = true;
            return Task.CompletedTask;
        };

        // act && assert
        await Assert.ThrowsAsync<ExitException>(
            () => middleware.InvokeAsync(middelwareContext.Object, next));
        isNextInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_NoEnabledEnvironment_Throws()
    {
        // arrange
        var testEnvironment = new EnvironmentDefinition(
            "test",
            false);
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);

        var featureCollection = new FeatureCollection();
        ConfigurationFeature configurationFeature = new(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            new ProjectDefinition(
                "Test",
                new[] { testEnvironment },
                Array.Empty<ComponentReferenceDefinition>(),
                Array.Empty<ComponentRepositoryDefinition>(),
                Array.Empty<VariableProviderDefinition>(),
                Array.Empty<ComponentProviderDefinition>(),
                Array.Empty<ConfigurationFileDefinition>(),
                Array.Empty<ProjectDefinition>(),
                null
            ),
            null,
            null
        );
        featureCollection.Set(configurationFeature);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        middelwareContext.SetupGet(x => x.Parameter).Returns(ParameterCollection.Empty());
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);

        var middleware = new EnvironmentMiddleware();
        bool isNextInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isNextInvoked = true;
            return Task.CompletedTask;
        };

        // act && assert
        await Assert.ThrowsAsync<ExitException>(
            () => middleware.InvokeAsync(middelwareContext.Object, next));
        isNextInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_MultipleEnabledEnvironment_Throws()
    {
        // arrange
        var testEnvironment1 = new EnvironmentDefinition(
            "tes1",
            true);
        var testEnvironment2 = new EnvironmentDefinition(
            "test2",
            true);
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);

        var featureCollection = new FeatureCollection();
        ConfigurationFeature configurationFeature = new(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            new ProjectDefinition(
                "Test",
                new[] { testEnvironment1, testEnvironment2 },
                Array.Empty<ComponentReferenceDefinition>(),
                Array.Empty<ComponentRepositoryDefinition>(),
                Array.Empty<VariableProviderDefinition>(),
                Array.Empty<ComponentProviderDefinition>(),
                Array.Empty<ConfigurationFileDefinition>(),
                Array.Empty<ProjectDefinition>(),
                null
            ),
            null,
            null
        );
        featureCollection.Set(configurationFeature);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        middelwareContext.SetupGet(x => x.Parameter).Returns(ParameterCollection.Empty());
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);

        var middleware = new EnvironmentMiddleware();
        bool isNextInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isNextInvoked = true;
            return Task.CompletedTask;
        };

        // act && assert
        await Assert.ThrowsAsync<ExitException>(
            () => middleware.InvokeAsync(middelwareContext.Object, next));
        isNextInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_Parameter_OverrideDefinition()
    {
        // arrange
        var testEnvironment1 = new EnvironmentDefinition(
            "test1",
            false);
        var testEnvironment2 = new EnvironmentDefinition(
            "test2",
            true);
        var middelwareContext = new Mock<IMiddlewareContext>(MockBehavior.Strict);

        var featureCollection = new FeatureCollection();
        ConfigurationFeature configurationFeature = new(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            new ProjectDefinition(
                "Test",
                new[] { testEnvironment1, testEnvironment2 },
                Array.Empty<ComponentReferenceDefinition>(),
                Array.Empty<ComponentRepositoryDefinition>(),
                Array.Empty<VariableProviderDefinition>(),
                Array.Empty<ComponentProviderDefinition>(),
                Array.Empty<ConfigurationFileDefinition>(),
                Array.Empty<ProjectDefinition>(),
                null
            ),
            null,
            null
        );
        featureCollection.Set(configurationFeature);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        var parameters = ParameterCollection.From(new Dictionary<Symbol, object?>() {
            { ActiveEnvironmentOption.Instance, "test1" }
        });

        middelwareContext.SetupGet(x => x.Parameter).Returns(parameters);
        middelwareContext.SetupGet(x => x.Logger).Returns(ConsoleLogger.NullLogger);

        var middleware = new EnvironmentMiddleware();
        bool isNextInvoked = false;
        MiddlewareDelegate next = _ =>
        {
            isNextInvoked = true;
            return Task.CompletedTask;
        };

        // act
        await middleware.InvokeAsync(middelwareContext.Object, next);

        // assert
        isNextInvoked.Should().BeTrue();
        var environmentFeature = featureCollection.Get<EnvironmentFeature>();
        environmentFeature.Should().NotBeNull();
        environmentFeature.ActiveEnvironment.Should().Be(testEnvironment1);
    }
}