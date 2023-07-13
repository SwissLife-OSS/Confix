using ConfiX.Entities.Component.Configuration;
using Confix.Tool;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;
using Confix.Tool.Middlewares;
using Moq;
using Snapshooter.Xunit;

namespace ConfiX.Inputs;

public class DonetComponentInputTests
{
    //[Fact]
    public async Task Should_ThrowExitException_When_ConfigurationScopeIsNotComponent()
    {
        // Arrange
        var context = CreateMiddlewareContext(
            TestHelpers.CreateConfigurationFeature(scope: ConfigurationScope.Solution));

        var componentInput = new DotnetComponentInput();

        // Act and Assert
        await Assert.ThrowsAsync<ExitException>(()
            => componentInput.ExecuteAsync(context));
    }

    [Fact]
    public async Task Should_ReturnEarly_When_ProjectFileNotFoundInDirectory()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = cli.Directories.Content.Append("Components").Append("test")
        };

        // Act
        //await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    //[Fact]
    public async Task Should_ReturnEarly_When_DotnetProjectWasNotFoundInDirectory()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.Directories.Content.CreateConfixProject();
        cli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = cli.Directories.Content.Append("Components").Append("test")
        };

        // Act
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    //[Fact]
    public async Task Should_LogError_When_ProjectFileCouldNotBeParsed()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.Directories.Content.CreateConfixProject();
        var csproj = cli.Directories.Content.CreateFileInPath("test.csproj", "");
        cli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = cli.Directories.Content.Append("Components").Append("test")
        };

        // Act
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).AddFile(csproj).MatchSnapshot();
    }

    //[Fact]
    public async Task Should_AddEmbeddedResources_When_DotnetProjectWasButNoEmbeddedFiles()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.Directories.Content.CreateConfixProject();
        var csproj = cli.Directories.Content.CreateFileInPath("test.csproj", "<Project></Project>");
        cli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = cli.Directories.Content.Append("Components").Append("test")
        };

        // Act
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).AddFile(csproj).MatchSnapshot();
    }

    //[Fact]
    public async Task Should_AddEmbeddedResources_When_DotnetProjectDoesNotHaveRoot()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.Directories.Content.CreateConfixProject();
        var csproj = cli.Directories.Content.CreateFileInPath("test.csproj", "");
        cli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = cli.Directories.Content.Append("Components").Append("test")
        };

        // Act
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).AddFile(csproj).MatchSnapshot();
    }

    //[Fact]
    public async Task Should_NotDoAnything_When_ProjectHasAlreadyEmbeddedFiles()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.Directories.Content.CreateConfixProject();
        const string projectContent = """
        <Project>
          <ItemGroup>
            <EmbeddedResource Include="$(MSBuildProjectDirectory)/Components/**/*.*" />
          </ItemGroup>
        </Project>
        """;
        var csproj = cli.Directories.Content.CreateFileInPath("test.csproj", projectContent);
        cli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = cli.Directories.Content.Append("Components").Append("test")
        };

        // Act
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).AddFile(csproj).MatchSnapshot();
    }

    private const string _confixRc = """
        {
            "component": 
            {
                 "inputs": [
                      {
                        "type": "dotnet"
                      }
                    ]
            }
        }
    """;

    private IMiddlewareContext CreateMiddlewareContext(ConfigurationFeature configurationFeature)
    {
        var fakeMiddlewareContext = new Mock<IMiddlewareContext>();
        var featureCollection = new FeatureCollection();

        featureCollection.Set(configurationFeature);
        fakeMiddlewareContext.SetupGet(x => x.Features).Returns(featureCollection);

        return fakeMiddlewareContext.Object;
    }
}
