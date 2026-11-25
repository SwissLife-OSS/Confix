using Confix.Inputs;

namespace Confix.Commands.Variables;

public class VariableListCommandTests
{
    [Theory]
    [InlineData("variable list")]
    [InlineData("variable list --provider local")]
    public async Task RunWithExitCodeAsync_ArrayVariables_PrintsLiteralPathsAsync(string command)
    {
        // Arrange
        using var cli = new TestConfixCommandline();
        cli.Directories.Content.CreateConfixRc(
            """
            {
              "isRoot": true,
              "project": {
                "variableProviders": [
                  {
                    "name": "local",
                    "type": "local",
                    "path": "./variables.json"
                  }
                ]
              }
            }
            """);
        cli.Directories.Content.CreateConfixSolution();
        cli.Directories.Content.CreateFileInPath(
            "variables.json",
            """
            {
              "scalar": "example",
              "items": ["first", "second"],
              "nested": [{"items": ["example"]}],
              "[red]": "example"
            }
            """);

        // Act
        int exitCode = await cli.RunWithExitCodeAsync(command.Split(' '));

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("$local:scalar", cli.Output);
        Assert.Contains("$local:items.[0]", cli.Output);
        Assert.Contains("$local:items.[1]", cli.Output);
        Assert.Contains("$local:nested.[0].items.[0]", cli.Output);
        Assert.Contains("$local:[red]", cli.Output);
    }
}
