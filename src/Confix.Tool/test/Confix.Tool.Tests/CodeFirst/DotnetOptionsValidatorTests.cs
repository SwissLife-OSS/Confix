using System.CommandLine;
using System.Text.Json.Nodes;
using Confix.ConfigurationFiles;
using Confix.Tool;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Validation;
using FluentAssertions;
using Moq;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers the guards the dotnet-options provider applies before running anything.</summary>
public sealed class DotnetOptionsValidatorTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("confix-validator-");

    public void Dispose() => _directory.Delete(recursive: true);

    [Fact]
    public async Task RequiredValidationCannotBeSkippedWithNoRestore()
    {
        var validate = async () => await ValidateAsync(
            new Dictionary<Symbol, object?> { [NoRestoreOptions.Instance] = true });

        (await validate.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("--no-restore is not supported");
    }

    [Fact]
    public async Task AProjectDirectoryWithoutACsprojIsRejected()
    {
        var validate = async () => await ValidateAsync();

        (await validate.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("exactly one project file");
    }

    [Fact]
    public async Task AnAmbiguousProjectDirectoryIsRejected()
    {
        await File.WriteAllTextAsync(Path.Combine(_directory.FullName, "a.csproj"), "<Project/>");
        await File.WriteAllTextAsync(Path.Combine(_directory.FullName, "b.csproj"), "<Project/>");

        var validate = async () => await ValidateAsync();

        (await validate.Should().ThrowAsync<ExitException>())
            .Which.Message.Should().Contain("exactly one project file");
    }

    [Fact]
    public void TheRunnerIsShippedNextToTheCli()
    {
        var runner = Path.Combine(AppContext.BaseDirectory, "Confix.Runner.dll");

        File.Exists(runner).Should().BeTrue();
    }

    private async Task ValidateAsync(IReadOnlyDictionary<Symbol, object?>? parameters = null)
    {
        var features = new FeatureCollection();
        features.Set(new ConfigurationFeature(
            ConfigurationScope.None,
            Mock.Of<IConfigurationFileCollection>(),
            ConfigurationInputTests.Project(_directory),
            null,
            null,
            null,
            null));
        features.Set(new ConfigurationFileFeature());

        var context = new Mock<IMiddlewareContext>(MockBehavior.Strict);
        context.SetupGet(c => c.Features).Returns(features);
        context.SetupGet(c => c.Logger).Returns(ConsoleLogger.NullLogger);
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
        context.SetupGet(c => c.Parameter).Returns(parameters is null
            ? ParameterCollection.Empty()
            : ParameterCollection.From(parameters));

        await new DotnetOptionsValidator().ValidateAsync(
            context.Object,
            new ValidationConfiguration(ValidationConfiguration.DotnetOptions),
            exportSchema: false);
    }
}

/// <summary>Covers that validate never mutates the project the way build does.</summary>
public sealed class ReadOnlyConfigurationTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("confix-readonly-");

    public void Dispose() => _directory.Delete(recursive: true);

    [Fact]
    public async Task ValidationDoesNotCreateUserSecretsForTheProject()
    {
        var files = await ResolveAsync(readOnly: true);

        files.Should().ContainSingle();
        files[0].OutputFile.FullName.Should().Be(files[0].InputFile.FullName);
        File.ReadAllText(Path.Combine(_directory.FullName, "app.csproj")).Should()
            .NotContain("UserSecretsId");
    }

    [Fact]
    public void TheReadOnlyFlagDefaultsToFalseForExistingContexts()
    {
        Mock.Of<IConfigurationFileContext>().ReadOnly.Should().BeFalse();

        new ConfigurationFileContext
        {
            Definition = new ConfigurationFileDefinition("appsettings", new JsonObject()),
            Project = ConfigurationInputTests.Project(_directory),
            Logger = ConsoleLogger.NullLogger
        }.ReadOnly.Should().BeFalse();
    }

    private async Task<IReadOnlyList<ConfigurationFile>> ResolveAsync(bool readOnly)
    {
        await File.WriteAllTextAsync(
            Path.Combine(_directory.FullName, "appsettings.json"),
            "{}");
        await File.WriteAllTextAsync(
            Path.Combine(_directory.FullName, "app.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup /></Project>");

        var context = new ConfigurationFileContext
        {
            ReadOnly = readOnly,
            Definition = new ConfigurationFileDefinition(
                "appsettings",
                new JsonObject { ["type"] = "appsettings", ["useUserSecrets"] = true }),
            Project = ConfigurationInputTests.Project(_directory),
            Logger = ConsoleLogger.NullLogger
        };

        return await new AppSettingsConfigurationFileProvider()
            .GetConfigurationFilesAsync(context, CancellationToken.None);
    }
}
