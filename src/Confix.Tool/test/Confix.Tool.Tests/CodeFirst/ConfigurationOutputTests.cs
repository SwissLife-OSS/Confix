using System.CommandLine;
using System.Text;
using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Encryption;
using Confix.Tool.Validation;
using FluentAssertions;
using Moq;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers staged publication of resolved output and the optional IDE schema export.</summary>
public sealed class ConfigurationOutputTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("confix-output-");

    public void Dispose() => _directory.Delete(recursive: true);

    [Fact]
    public async Task ResolvedContentIsWrittenIndentedToTheOutputFile()
    {
        var file = File("out.json", "{\"Mail\":{\"Host\":\"server\"}}");

        await PublishAsync(file);

        var written = await System.IO.File.ReadAllTextAsync(file.OutputFile.FullName);

        written.Should().Contain("\n").And.Contain("\"Host\": \"server\"");
        JsonNode.Parse(written)!["Mail"]!["Host"]!.GetValue<string>().Should().Be("server");
    }

    [Fact]
    public async Task MissingOutputDirectoriesAreCreated()
    {
        var file = File(Path.Combine("nested", "deeper", "out.json"), "{}");

        await PublishAsync(file);

        System.IO.File.Exists(file.OutputFile.FullName).Should().BeTrue();
    }

    [Fact]
    public async Task ExistingOutputIsReplaced()
    {
        var file = File("out.json", "{\"New\":1}");
        await System.IO.File.WriteAllTextAsync(file.OutputFile.FullName, "{\"Old\":1}");

        await PublishAsync(file);

        var written = await System.IO.File.ReadAllTextAsync(file.OutputFile.FullName);

        written.Should().Contain("New").And.NotContain("Old");
    }

    [Fact]
    public async Task TwoInputsCannotShareOneDestination()
    {
        var first = File("same.json", "{\"A\":1}");
        var second = File("same.json", "{\"B\":2}");

        var publish = async () => await PublishAsync(first, second);

        (await publish.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("share one output destination");
    }

    [Fact]
    public async Task NoTemporaryFilesSurvivePublication()
    {
        var file = File("out.json", "{}");

        await PublishAsync(file);

        _directory.EnumerateFiles("*.tmp", SearchOption.AllDirectories).Should().BeEmpty();
    }

    [Fact]
    public async Task AFailedBatchLeavesPreviousOutputsUntouched()
    {
        var first = File("first.json", "{\"A\":1}");
        var duplicate = File("first.json", "{\"B\":2}");
        await System.IO.File.WriteAllTextAsync(first.OutputFile.FullName, "{\"Original\":1}");

        var publish = async () => await PublishAsync(first, duplicate);

        await publish.Should().ThrowAsync<ExitException>();

        var content = await System.IO.File.ReadAllTextAsync(first.OutputFile.FullName);

        content.Should().Be("{\"Original\":1}");
        _directory.EnumerateFiles("*.tmp", SearchOption.AllDirectories).Should().BeEmpty();
    }

    [Fact]
    public async Task OutputIsNotReadableByOtherUsers()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var file = File("out.json", "{}");

        await PublishAsync(file);

        var mode = System.IO.File.GetUnixFileMode(file.OutputFile.FullName);

        mode.Should().NotHaveFlag(UnixFileMode.GroupRead).And.NotHaveFlag(UnixFileMode.OtherRead);
    }

    [Fact]
    public async Task EncryptionIsAppliedWhenRequested()
    {
        var file = File("out.json", "{\"Secret\":\"value\"}");
        var encryption = new Mock<IEncryptionProvider>();
        encryption
            .Setup(p => p.EncryptAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes("encrypted"));

        await PublishAsync(
            [file],
            parameters: new Dictionary<Symbol, object?> { [EncryptionOption.Instance] = true },
            encryption: new EncryptionFeature(encryption.Object));

        var written = await System.IO.File.ReadAllTextAsync(file.OutputFile.FullName);

        written.Should().Be("encrypted").And.NotContain("value");
    }

    [Fact]
    public async Task RequestingEncryptionWithoutAProviderFails()
    {
        var file = File("out.json", "{}");

        var publish = async () => await PublishAsync(
            [file],
            parameters: new Dictionary<Symbol, object?> { [EncryptionOption.Instance] = true });

        (await publish.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("Encryption must be configured");
    }

    [Fact]
    public async Task TheSchemaExportWritesTheFileAndTheEditorAssociation()
    {
        var schema = JsonNode.Parse("{\"type\":\"object\"}")!;

        await ConfigurationOutput.ExportSchemaAsync(_directory.FullName, schema, default);

        var exported = Path.Combine(_directory.FullName, ConfigurationOutput.SchemaFileName);
        var settings = JsonNode.Parse(
            await System.IO.File.ReadAllTextAsync(SettingsPath()))!;

        System.IO.File.Exists(exported).Should().BeTrue();

        var schemas = settings["json.schemas"]!.AsArray();

        schemas.Should().ContainSingle();
        schemas[0]!["url"]!.GetValue<string>().Should().Be("./" + ConfigurationOutput.SchemaFileName);
        schemas[0]!["fileMatch"]!.AsArray()[0]!.GetValue<string>().Should().Be("/appsettings*.json");
    }

    [Fact]
    public async Task ExistingEditorSettingsArePreserved()
    {
        Directory.CreateDirectory(Path.Combine(_directory.FullName, ".vscode"));
        await System.IO.File.WriteAllTextAsync(SettingsPath(), """
            {
                // a comment tolerated by VS Code
                "editor.tabSize": 2,
                "json.schemas": [{ "url": "./other.json", "fileMatch": ["/other.json"] }],
            }
            """);

        await ConfigurationOutput.ExportSchemaAsync(_directory.FullName, Schema(), default);

        var settings = JsonNode.Parse(await System.IO.File.ReadAllTextAsync(SettingsPath()))!;

        settings["editor.tabSize"]!.GetValue<int>().Should().Be(2);
        settings["json.schemas"]!.AsArray().Should().HaveCount(2);
    }

    [Fact]
    public async Task ExportingTwiceDoesNotDuplicateTheAssociation()
    {
        await ConfigurationOutput.ExportSchemaAsync(_directory.FullName, Schema(), default);
        await ConfigurationOutput.ExportSchemaAsync(_directory.FullName, Schema(), default);

        var settings = JsonNode.Parse(await System.IO.File.ReadAllTextAsync(SettingsPath()))!;

        settings["json.schemas"]!.AsArray().Should().ContainSingle();
    }

    private static JsonNode Schema() => JsonNode.Parse("{\"type\":\"object\"}")!;

    private string SettingsPath() => Path.Combine(_directory.FullName, ".vscode", "settings.json");

    private ConfigurationFile File(string relativeOutput, string json)
    {
        var input = Path.Combine(_directory.FullName, "appsettings.json");

        return new ConfigurationFile
        {
            InputFile = new FileInfo(input),
            OutputFile = new FileInfo(Path.Combine(_directory.FullName, relativeOutput)),
            Content = JsonNode.Parse(json)
        };
    }

    private static Task PublishAsync(params ConfigurationFile[] files)
    {
        return PublishAsync(files, null, null);
    }

    private static async Task PublishAsync(
        ConfigurationFile[] files,
        IReadOnlyDictionary<Symbol, object?>? parameters = null,
        EncryptionFeature? encryption = null)
    {
        var features = new FeatureCollection();
        var feature = new ConfigurationFileFeature();

        foreach (var file in files)
        {
            feature.Files.Add(file);
        }

        features.Set(feature);

        if (encryption is not null)
        {
            features.Set(encryption);
        }

        var context = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        context.SetupGet(c => c.Features).Returns(features);
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
        context.SetupGet(c => c.Parameter).Returns(parameters is null
            ? ParameterCollection.Empty()
            : ParameterCollection.From(parameters));

        await ConfigurationOutput.PublishAsync(context.Object);
    }
}
