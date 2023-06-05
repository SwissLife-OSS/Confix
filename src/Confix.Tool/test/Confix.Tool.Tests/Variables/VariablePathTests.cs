using ConfiX.Variables;
using FluentAssertions;
namespace Confix.Tool.Tests;

public class VariablePathTests
{
    [Fact]
    public void Parse_ValidVariableName_CorrectResult()
    {
        // arrange & act
        VariablePath result = VariablePath.Parse("$foo.bar:baz.x");

        // assert
        result.ProviderName.Should().Be("foo.bar");
        result.Path.Should().Be("baz.x");
    }

    [Theory]
    [InlineData("$foo:bar:baz")]
    [InlineData("bar")]
    [InlineData("$foo.bar")]
    [InlineData("foo:bar")]
    public void Parse_Invalid_Throws(string variableName)
    {
        // act & assert
        Assert.Throws<VariablePathParseException>(() => VariablePath.Parse(variableName));
    }
}