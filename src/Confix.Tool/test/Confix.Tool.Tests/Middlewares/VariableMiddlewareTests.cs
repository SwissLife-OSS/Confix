using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Variables;
using FluentAssertions;
using Moq;

namespace Confix.Entities.Component.Configuration.Middlewares;

public class VariableMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Should_SetVariableResolverFeatureAsync()
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
                new VariableProviderDefinition[]{
                    new (
                        "test",
                        "test",
                        new Dictionary<string, JsonObject>(){
                            {"test", JsonNode.Parse("""{"path": "./override.json"}""")!.AsObject()}
                        },
                        JsonNode.Parse("""{"path": "./test.json"}""")!.AsObject()
                    )
                },
                Array.Empty<ComponentProviderDefinition>(),
                Array.Empty<ConfigurationFileDefinition>(),
                Array.Empty<ProjectDefinition>(),
                ProjectType.Default,
                null
            ),
            null,
            null,
            null
        );

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(p => p.GetService(typeof(VariableListCache)))
            .Returns(new VariableListCache());
        
        featureCollection.Set(configurationFeature);
        EnvironmentFeature environmentFeature = new(new EnvironmentDefinition("test", true));
        featureCollection.Set(environmentFeature);
        middelwareContext.SetupGet(x => x.Features).Returns(featureCollection);
        middelwareContext.SetupGet(x => x.Services).Returns(serviceProviderMock.Object);

        Mock<IVariableProviderFactory> factoryMock = new(MockBehavior.Strict);
        var middleware = new VariableMiddleware(factoryMock.Object);
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
        var variableResolverFeature = featureCollection.Get<VariablesFeature>();
        variableResolverFeature.Should().NotBeNull();
        variableResolverFeature.Resolver.Should().NotBeNull();
    }
}
