using Confix.Variables;
using FluentAssertions;
namespace Confix.Tool.Tests;

public class VariablePathTests
{
    [Theory]
    [InlineData("$foo:bar", "foo", "bar")]
    [InlineData("$foo.bar:baz.x", "foo.bar", "baz.x")]
    [InlineData("$foo_bar:baz_x", "foo_bar", "baz_x")]
    [InlineData("$secret:K2b8F2zG9HpJxMI", "secret", "K2b8F2zG9HpJxMI")]
    [InlineData("$secret:abc+def/ghi==", "secret", "abc+def/ghi==")]
    [InlineData("$secret:abc/def+ghi=", "secret", "abc/def+ghi=")]
    [InlineData("$secret:abc-def_ghi", "secret", "abc-def_ghi")]
    public void Parse_ValidVariableName_CorrectResult(string variableName, string providerName, string path)
    {
        // arrange & act
        VariablePath result = VariablePath.Parse(variableName);

        // assert
        result.ProviderName.Should().Be(providerName);
        result.Path.Should().Be(path);
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

    [Fact]
    public void TryParse_Base64EncodedPath_CorrectResult()
    {
        // arrange
        const string base64 = "K2b8F2zG9HpJxMImaYwlf0ByzArc+abc/def==";

        // act
        bool success = VariablePath.TryParse($"$secret:{base64}", out VariablePath? path);

        // assert
        success.Should().BeTrue();
        path.HasValue.Should().BeTrue();
        path!.Value.ProviderName.Should().Be("secret");
        path.Value.Path.Should().Be(base64);
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

    [Fact]
    public void GetVariables_Base64EncodedPath_ReturnsFullVariable()
    {
        // arrange
        const string base64 = "K2b8F2zG9HpJxMImaYwlf0ByzArc+abc/def==";
        string value = $"$secret:{base64}";

        // act
        var variables = value.GetVariables().ToArray();

        // assert
        variables.Should().HaveCount(1);
        variables[0].ProviderName.Should().Be("secret");
        variables[0].Path.Should().Be(base64);
    }

    [Fact]
    public void GetVariables_InterpolatedBase64EncodedPath_ReturnsFullVariable()
    {
        // arrange
        const string base64 = "K2b8F2zG9HpJxMImaYwlf0ByzArc+abc/def==";
        string value = $"prefix-{{{{$secret:{base64}}}}}-suffix";

        // act
        var variables = value.GetVariables().ToArray();

        // assert
        variables.Should().HaveCount(1);
        variables[0].ProviderName.Should().Be("secret");
        variables[0].Path.Should().Be(base64);
    }

    [Fact]
    public void ReplaceVariables_Base64EncodedPath_ReplacesFullVariable()
    {
        // arrange
        const string base64 = "K2b8F2zG9HpJxMImaYwlf0ByzArc+abc/def==";
        string value = $"$secret:{base64}";

        // act
        string result = value.ReplaceVariables(_ => "REPLACED");

        // assert
        result.Should().Be("REPLACED");
    }
}