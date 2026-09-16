using System.CommandLine;
using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Validation;
using FluentAssertions;
using Moq;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers overlay composition and in-place output redirection.</summary>
public sealed class ConfigurationInputTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("confix-input-");

    public void Dispose() => _directory.Delete(recursive: true);

    [Fact]
    public async Task WithoutOverlaysTheInputIsUsedAsIs()
    {
        var file = Input("{\"Mail\":{\"Host\":\"server\"}}");

        await ApplyAsync(new ValidationConfiguration(), file);

        file.Content!.ToJsonString().Should().Be("{\"Mail\":{\"Host\":\"server\"}}");
    }

    [Fact]
    public async Task OverlaysMergeObjectsByKey()
    {
        var file = Input("{\"Mail\":{\"Host\":\"server\",\"Port\":25}}");
        Overlay("overlay.json", "{\"Mail\":{\"Port\":587},\"Extra\":{\"A\":1}}");

        await ApplyAsync(Settings("overlay.json"), file);

        var merged = file.Content!.AsObject();

        merged["Mail"]!["Host"]!.GetValue<string>().Should().Be("server");
        merged["Mail"]!["Port"]!.GetValue<int>().Should().Be(587);
        merged["Extra"]!["A"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public async Task OverlaysReplaceArraysScalarsAndNull()
    {
        var file = Input("{\"Items\":[1,2],\"Scalar\":\"a\",\"Object\":{\"Keep\":1}}");
        Overlay("overlay.json", "{\"Items\":[3],\"Scalar\":\"b\",\"Object\":null}");

        await ApplyAsync(Settings("overlay.json"), file);

        var merged = file.Content!.AsObject();

        merged["Items"]!.AsArray().Should().HaveCount(1);
        merged["Scalar"]!.GetValue<string>().Should().Be("b");
        merged["Object"].Should().BeNull();
    }

    [Fact]
    public async Task OverlayKeysAreMatchedCaseInsensitively()
    {
        var file = Input("{\"Mail\":{\"Host\":\"server\"}}");
        Overlay("overlay.json", "{\"mail\":{\"host\":\"other\"}}");

        await ApplyAsync(Settings("overlay.json"), file);

        var merged = file.Content!.AsObject();

        merged.Should().ContainSingle();
        merged["Mail"]!["Host"]!.GetValue<string>().Should().Be("other");
    }

    [Fact]
    public async Task OverlaysAreAppliedInOrder()
    {
        var file = Input("{\"Value\":0}");
        Overlay("first.json", "{\"Value\":1}");
        Overlay("second.json", "{\"Value\":2}");

        await ApplyAsync(Settings("first.json", "second.json"), file);

        file.Content!["Value"]!.GetValue<int>().Should().Be(2);
    }

    [Fact]
    public async Task TheEnvironmentPlaceholderIsSubstituted()
    {
        var file = Input("{\"Value\":0}");
        Overlay("appsettings.dev.json", "{\"Value\":42}");

        await ApplyAsync(Settings("appsettings.{environment}.json"), file, environment: "dev");

        file.Content!["Value"]!.GetValue<int>().Should().Be(42);
    }

    [Fact]
    public async Task AMissingOverlayFailsWithItsDeclaredName()
    {
        var file = Input("{}");

        var apply = async () => await ApplyAsync(Settings("absent.json"), file);

        (await apply.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("absent.json").And.Contain("does not exist");
    }

    [Fact]
    public async Task OverlaysCannotEscapeTheProjectDirectory()
    {
        var file = Input("{}");

        var apply = async () => await ApplyAsync(Settings("../outside.json"), file);

        (await apply.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("inside the project directory");
    }

    [Fact]
    public async Task UnparsableOverlaysAreReportedWithoutTheirContent()
    {
        var file = Input("{}");
        Overlay("broken.json", "{ not json");

        var apply = async () => await ApplyAsync(Settings("broken.json"), file);

        (await apply.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("broken.json").And.NotContain("not json");
    }

    [Fact]
    public async Task NonObjectOverlaysAreRejected()
    {
        var file = Input("{}");
        Overlay("array.json", "[1,2]");

        var apply = async () => await ApplyAsync(Settings("array.json"), file);

        (await apply.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("must be JSON objects");
    }

    [Fact]
    public async Task InPlaceOutputIsRedirectedToASidecarFile()
    {
        var file = Input("{}");

        await ApplyAsync(new ValidationConfiguration(), file);

        file.OutputFile.Name.Should().Be("appsettings.confix.json");
        file.OutputFile.DirectoryName.Should().Be(_directory.FullName);
    }

    [Fact]
    public async Task AnExplicitOutputFileSuppressesTheSidecarRedirect()
    {
        var file = Input("{}");
        var requested = new FileInfo(Path.Combine(_directory.FullName, "explicit.json"));

        await ApplyAsync(
            new ValidationConfiguration(),
            file,
            parameters: new Dictionary<Symbol, object?> { [OutputFileOption.Instance] = requested });

        file.OutputFile.FullName.Should().Be(file.InputFile.FullName);
    }

    [Fact]
    public async Task ADistinctOutputDestinationIsLeftAlone()
    {
        var file = Input("{}");
        file.OutputFile = new FileInfo(Path.Combine(_directory.FullName, "secrets.json"));

        await ApplyAsync(new ValidationConfiguration(), file);

        file.OutputFile.Name.Should().Be("secrets.json");
    }

    private static ValidationConfiguration Settings(params string[] overlays)
    {
        return new ValidationConfiguration(ValidationConfiguration.DotnetOptions, Overlays: overlays);
    }

    private ConfigurationFile Input(string json)
    {
        var path = Path.Combine(_directory.FullName, "appsettings.json");
        File.WriteAllText(path, json);

        return new ConfigurationFile
        {
            InputFile = new FileInfo(path),
            OutputFile = new FileInfo(path)
        };
    }

    private void Overlay(string name, string json)
    {
        File.WriteAllText(Path.Combine(_directory.FullName, name), json);
    }

    private async Task ApplyAsync(
        ValidationConfiguration settings,
        ConfigurationFile file,
        string environment = "prod",
        IReadOnlyDictionary<Symbol, object?>? parameters = null)
    {
        var features = new FeatureCollection();
        features.Set(new ConfigurationFeature(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            Project(_directory),
            null,
            null,
            null,
            null));
        features.Set(new EnvironmentFeature(new EnvironmentDefinition(environment, true)));
        features.Set(new ConfigurationFileFeature { Files = { file } });

        var context = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        context.SetupGet(c => c.Features).Returns(features);
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
        context.SetupGet(c => c.Parameter).Returns(parameters is null
            ? ParameterCollection.Empty()
            : ParameterCollection.From(parameters));

        await ConfigurationInput.ApplyAsync(context.Object, settings);
    }

    internal static ProjectDefinition Project(DirectoryInfo directory)
    {
        return new ProjectDefinition(
            "test",
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            ProjectType.Default,
            directory);
    }
}
