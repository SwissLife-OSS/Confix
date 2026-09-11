using Confix.Tool;
using Confix.Tool.Commands.Logging;
using FluentAssertions;
using Spectre.Console.Testing;

namespace Confix.CodeFirst.Tests;

public sealed class LoggerMessageTests
{
    [Theory]
    [InlineData("Overlay 'appsettings.{environment}.json' does not exist.")]
    [InlineData("Section {0} is unknown.")]
    [InlineData("Unbalanced } brace.")]
    public void LiteralMessagesAreNotTreatedAsFormatStrings(string message)
    {
        var console = new TestConsole();

        var write = () => new DefaultLoggerMessage
        {
            Template = message,
            Verbosity = Verbosity.Normal
        }.WriteTo(console);

        write.Should().NotThrow();
        console.Output.Should().Contain(message);
    }

    [Fact]
    public void TemplatesWithArgumentsStillFormat()
    {
        var console = new TestConsole();

        new DefaultLoggerMessage
        {
            Template = "Validated {0} files.",
            Verbosity = Verbosity.Normal,
            Arguments = [3]
        }.WriteTo(console);

        console.Output.Should().Contain("Validated 3 files.");
    }
}
