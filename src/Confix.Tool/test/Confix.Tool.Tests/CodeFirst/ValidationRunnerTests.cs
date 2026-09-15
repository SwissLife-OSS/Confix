using System.Text.Json.Nodes;
using Confix.Runner;
using FluentAssertions;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers the stdin/stdout protocol and failure reporting of the validation runner.</summary>
public sealed class ValidationRunnerTests
{
    [Fact]
    public async Task AMissingAssemblyArgumentIsReportedAsAProtocolError()
    {
        var (exit, response) = await RunAsync([], Request());

        exit.Should().Be(2);
        Errors(response).Should().ContainSingle().Which.Should().Contain("single assembly path");
    }

    [Fact]
    public async Task AnUnsupportedRequestVersionIsRejected()
    {
        var request = RequestNode();
        request["version"] = 2;

        var (exit, response) = await RunAsync([Self], request.ToJsonString());

        exit.Should().Be(2);
        Errors(response).Should().ContainSingle().Which.Should().Contain("protocol");
    }

    [Fact]
    public async Task MalformedRequestsDoNotLeakExceptionDetail()
    {
        var (exit, response) = await RunAsync([Self], "not json at all");

        exit.Should().Be(2);
        Errors(response).Should().ContainSingle().Which.Should().Contain("Validation runner failed");
    }

    [Fact]
    public async Task ARequestWithoutConfigurationIsRejected()
    {
        var (exit, response) = await RunAsync([Self], "{\"version\":1}");

        exit.Should().Be(2);
        Errors(response).Should().ContainSingle().Which.Should().Contain("no configuration");
    }

    [Fact]
    public async Task AnAssemblyWithoutAGeneratedCatalogExplainsTheMissingReference()
    {
        var (exit, response) = await RunAsync([Self], Request());

        exit.Should().Be(2);
        Errors(response).Should().ContainSingle()
            .Which.Should().Contain("Confix.CodeGeneration");
    }

    [Fact]
    public async Task AnApplicationWithoutConfixOptionsIsNamedExplicitly()
    {
        // A framework assembly resolves no Confix runtime packages at all.
        var runtime = typeof(object).Assembly.Location;

        var (exit, response) = await RunAsync([runtime], Request());

        exit.Should().Be(2);
        Errors(response).Should().ContainSingle()
            .Which.Should().Contain("does not reference").And.Contain("Confix");
    }

    [Fact]
    public async Task TheResponseAlwaysUsesTheCamelCaseWireFormat()
    {
        var (_, response) = await RunAsync([], Request());

        var document = JsonNode.Parse(response)!.AsObject();

        document.Should().ContainKey("version").And.ContainKey("errors");
        document["version"]!.GetValue<int>().Should().Be(1);
    }

    [Theory]
    [InlineData("2.1.0.0", "2.1.0.0", true)]
    [InlineData("2.0.0.0", "2.1.0.0", true)]
    [InlineData("2.1.0.0", "2.2.0.0", true)]
    [InlineData("2.2.0.0", "2.1.0.0", false)]
    [InlineData("1.9.0.0", "2.0.0.0", false)]
    [InlineData("3.0.0.0", "2.9.0.0", false)]
    public void OlderMinorsOfTheSameMajorAreCompatibleWithTheCli(
        string application,
        string cli,
        bool compatible)
    {
        ValidationRunner.IsCompatible(Version.Parse(application), Version.Parse(cli))
            .Should().Be(compatible);
    }

    [Fact]
    public void UnknownVersionsAreNeverCompatible()
    {
        ValidationRunner.IsCompatible(null, new Version(2, 1)).Should().BeFalse();
        ValidationRunner.IsCompatible(new Version(2, 1), null).Should().BeFalse();
    }

    private static string Self => typeof(ValidationRunnerTests).Assembly.Location;

    private static JsonObject RequestNode()
    {
        return new JsonObject
        {
            ["version"] = 1,
            ["configuration"] = new JsonObject(),
            ["coverage"] = "strict",
            ["exportSchema"] = false
        };
    }

    private static string Request() => RequestNode().ToJsonString();

    private static async Task<(int Exit, string Response)> RunAsync(string[] args, string request)
    {
        await using var output = new StringWriter();

        var exit = await ValidationRunner.RunAsync(args, request, output);

        return (exit, output.ToString());
    }

    private static IEnumerable<string> Errors(string response)
    {
        return JsonNode.Parse(response)!["errors"]!.AsArray().Select(e => e!.GetValue<string>());
    }
}
