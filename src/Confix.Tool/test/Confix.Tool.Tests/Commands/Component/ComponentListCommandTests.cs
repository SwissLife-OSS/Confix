using Confix.Inputs;
using Confix.Tool.Commands.Temp;

namespace Confix.Commands.Component;

public class ComponentListCommandTests
{
    [Fact]
    public async Task Should_OutputComponentList_When_Empty()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixProject();
        cli.Directories.Content.CreateConfixSolution();
        cli.Directories.Content.CreateConsoleApp();

        // Act
        await cli.RunAsync("component list");

        // Assert
        SnapshotBuilder
            .New()
            .AddOutput(cli)
            .AddReplacement(cli.Directories.Home.FullName, "HOME")
            .AddReplacement(cli.Directories.Content.FullName, "CONTENT")
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_OutputComponentList_When_Provider()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.Directories.Content.CreateConfixProject();
        cli.Directories.Content.CreateConfixSolution();
        cli.Directories.Content.CreateConsoleApp();
        var componentDirectory = cli.Directories.Content.Append("Components").Append("test");
        componentDirectory.CreateFileInPath("schema.graphql", "type Query { str: String }");

        // Act
        await BuildComponentAsync(cli);
        await cli.RunAsync("component list");

        // Assert
        SnapshotBuilder
            .New()
            .AddOutput(cli)
            .AddReplacement(cli.Directories.Home.FullName, "HOME")
            .AddReplacement(cli.Directories.Content.FullName, "CONTENT")
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_OutputComponentList_When_OutputAsJson()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.Directories.Content.CreateConfixProject();
        cli.Directories.Content.CreateConfixSolution();
        cli.Directories.Content.CreateConsoleApp();
        var componentDirectory = cli.Directories.Content.Append("Components").Append("test");
        componentDirectory.CreateFileInPath("schema.graphql", "type Query { str: String }");

        // Act
        await BuildComponentAsync(cli);
        await cli.RunAsync("component list --format json");

        // Assert
        SnapshotBuilder
            .New()
            .Append("output", cli.Console.Output)
            .AddReplacement(cli.Directories.Home.FullName, "HOME")
            .AddReplacement(cli.Directories.Content.FullName, "CONTENT")
            .MatchSnapshot();
    }

    private async Task BuildComponentAsync(TestConfixCommandline cli)
    {
        var otherCli = new TestConfixCommandline();
        otherCli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = cli.ExecutionContext.CurrentDirectory
                .Append("Components")
                .Append("test")
        };
        await otherCli.RunAsync("component build");
    }

    private const string _confixRc = """
            {
                "isRoot": true,
                "component":
                {
                     "inputs": [
                          {
                            "type": "graphql"
                          },
                          {
                            "type": "dotnet"
                          }
                        ]
                },
                "project": {
                     "componentProviders": [
                          {
                            "name": "dotnet-package",
                            "type": "dotnet-package"
                          }
                        ]
                }
            }
        """;
}
