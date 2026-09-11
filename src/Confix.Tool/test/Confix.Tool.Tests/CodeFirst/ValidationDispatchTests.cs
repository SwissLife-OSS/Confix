using Confix.Tool;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Validation;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.CodeFirst.Tests;

public sealed class ValidationDispatchTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("json-schema", null)]
    [InlineData("dotnet-options", "json-schema")]
    public async Task LegacySelectionContinuesExistingPipeline(string? projectProvider, string? environmentProvider)
    {
        var features = new FeatureCollection();
        var project = new ProjectDefinition("test", [], [], [], [], [], [], [], ProjectType.Default, null,
            projectProvider is null ? null : new ValidationConfiguration(projectProvider));
        features.Set(new ConfigurationFeature(ConfigurationScope.None, Mock.Of<IConfigurationFileCollection>(),
            project, null, null, null, null));
        features.Set(new EnvironmentFeature(new EnvironmentDefinition("test", true,
            environmentProvider is null ? null : new ValidationConfiguration(environmentProvider))));
        using var services = new ServiceCollection().AddSingleton<EnvironmentMiddleware>().BuildServiceProvider();
        var context = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        context.SetupGet(c => c.Services).Returns(services);
        context.SetupGet(c => c.Status).Returns(Mock.Of<IStatus>());
        context.SetupGet(c => c.Features).Returns(features);
        context.SetupGet(c => c.Parameter).Returns(ParameterCollection.Empty());
        context.SetupGet(c => c.Logger).Returns(ConsoleLogger.NullLogger);
        var continued = false;
        await ConfigurationValidationPipeline.DispatchAsync(context.Object, _ =>
        {
            continued = true;
            return Task.CompletedTask;
        }, write: true);
        continued.Should().BeTrue();
        features.TryGet<ConfigurationReadOnlyFeature>(out _).Should().BeFalse();
    }

    [Fact]
    public void SupersededDraftConfigurationDoesNotSilentlySelectLegacyValidation()
    {
        Action parse = () => ProjectConfiguration.Parse(System.Text.Json.Nodes.JsonNode.Parse("""{"codeFirst":{"enabled":true}}"""));
        parse.Should().Throw<ArgumentException>().WithMessage("*validation*");
    }
}
