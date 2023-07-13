using Confix.Tool.Commands.Temp;

namespace ConfiX.Inputs;

public class GraphQlComponentInputTests
{
    //[Fact]
    public async Task Should_Fail_When_NotExecuted_In_ComponentDirectory()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        cli.ExecutionContext = cli.ExecutionContext;

        // Act
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    //[Fact]
    public async Task Should_ReturnEarly_When_SchemaGraphQlNotFoundInDirectory()
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
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New().AddOutput(cli).MatchSnapshot();
    }

    //[Fact]
    public async Task Should_GenerateSchemaJson_When_SchemaGraphQlFoundInDirectory()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        var componentDirectory = cli.Directories.Content.Append("Components").Append("test");
        componentDirectory.CreateFileInPath("schema.graphql", "type Query { str: String }");
        cli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = componentDirectory
        };

        // Act
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New()
            .AddOutput(cli)
            .AddFile(componentDirectory.Append("schema.json").FullName)
            .MatchSnapshot();
    }

    //[Fact]
    public async Task Should_Should_ReplaceExistingSchemaFile_When_ItAlreadyExists()
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixComponent("test");
        var componentDirectory = cli.Directories.Content.Append("Components").Append("test");
        componentDirectory.CreateFileInPath("schema.graphql", "type Query { str: String }");
        componentDirectory.CreateFileInPath("schema.json", "SHOULD BE REPLACED");
        cli.ExecutionContext = cli.ExecutionContext with
        {
            CurrentDirectory = componentDirectory
        };

        // Act
        await cli.RunAsync("component build");

        // Assert
        SnapshotBuilder.New()
            .AddOutput(cli)
            .AddFile(componentDirectory.Append("schema.json").FullName)
            .MatchSnapshot();
    }

    private const string _confixRc = """
        {
            "component": 
            {
                 "inputs": [
                      {
                        "type": "graphql"
                      }
                    ]
            }
        }
    """;
}
