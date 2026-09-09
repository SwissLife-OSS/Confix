using Confix.Inputs;

namespace Confix.Commands;

public class RootCommandTests
{
    [Fact]
    public async Task Help_ExitsSuccessfully()
    {
        using var cli = new TestConfixCommandline();

        var exitCode = await cli.RunWithExitCodeAsync("--help");

        Assert.Equal(0, exitCode);
        Assert.Contains("Usage:", cli.Console.Output);
    }

    [Fact]
    public async Task Version_ExitsSuccessfully()
    {
        using var cli = new TestConfixCommandline();

        var exitCode = await cli.RunWithExitCodeAsync("--version");

        Assert.Equal(0, exitCode);
        Assert.Matches(@"\d+\.\d+\.\d+", cli.Console.Output);
    }

    [Fact]
    public async Task UnknownOption_ExitsWithError()
    {
        using var cli = new TestConfixCommandline();

        var exitCode = await cli.RunWithExitCodeAsync("--does-not-exist");

        Assert.NotEqual(0, exitCode);
        Assert.Contains("Unrecognized", cli.Console.Output);
    }
}
