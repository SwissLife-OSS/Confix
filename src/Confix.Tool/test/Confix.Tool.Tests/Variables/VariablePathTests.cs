using Confix.Variables;
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

    [Fact]
    public void TryParse_ValidVariableName_CorrectResult()
    {
        // arrange & act
        bool success = VariablePath.TryParse("$foo.bar:baz.x", out VariablePath? path);

        // assert
        success.Should().BeTrue();
        path.HasValue.Should().BeTrue();
        path!.Value.ProviderName.Should().Be("foo.bar");
        path!.Value.Path.Should().Be("baz.x");
    }

    [Theory]
    [InlineData("$foo:bar:baz")]
    [InlineData("bar")]
    [InlineData("$foo.bar")]
    [InlineData("foo:bar")]
    public void TryParse_Invalid_SuccessFalse(string variableName)
    {
        // arrange & act
        bool success = VariablePath.TryParse(variableName, out VariablePath? path);

        // assert
        success.Should().BeFalse();
        path.HasValue.Should().BeFalse();
    }
}