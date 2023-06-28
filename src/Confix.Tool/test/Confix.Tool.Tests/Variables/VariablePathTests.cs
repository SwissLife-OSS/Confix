using ConfiX.Variables;
using FluentAssertions;
namespace Confix.Tool.Tests;

public class VariablePathTests
{
    [Theory]
    [InlineData("$foo:bar", "foo", "bar", null)]
    [InlineData("$foo:bar:baz", "foo", "bar", "baz")]
    [InlineData("$foo.bar:baz.x", "foo.bar", "baz.x", null)]
    [InlineData("$foo.bar:baz.x:suffix", "foo.bar", "baz.x", "suffix")]
    [InlineData("$foo.bar:baz.x:suffix:with:colon", "foo.bar", "baz.x", "suffix:with:colon")]
    public void Parse_ValidVariableName_CorrectResult(string variableName, string providerName, string path, string? suffix)
    {
        // arrange & act
        VariablePath result = VariablePath.Parse(variableName);

        // assert
        result.ProviderName.Should().Be(providerName);
        result.Path.Should().Be(path);
        result.Suffix.Should().Be(suffix);
    }

    [Theory]
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
        path!.Value.Suffix.Should().BeNull();
    }

    [Theory]
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