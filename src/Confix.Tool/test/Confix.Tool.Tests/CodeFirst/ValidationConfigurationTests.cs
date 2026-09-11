using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using FluentAssertions;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers parsing, inheritance and serialization of the validation settings.</summary>
public sealed class ValidationConfigurationTests
{
    [Fact]
    public void AbsentValidationDefaultsToTheExistingWorkflow()
    {
        ValidationConfiguration.Parse(null).Should().BeNull();
        new ValidationConfiguration().EffectiveType.Should().Be(ValidationConfiguration.JsonSchema);
    }

    [Fact]
    public void AllSupportedSettingsAreParsed()
    {
        var parsed = Parse("""
            {"type":"dotnet-options","coverage":"registeredSections","framework":"net10.0",
             "overlays":["a.json","b.json"]}
            """);

        parsed.Type.Should().Be(ValidationConfiguration.DotnetOptions);
        parsed.Coverage.Should().Be(ValidationConfiguration.RegisteredSections);
        parsed.Framework.Should().Be("net10.0");
        parsed.Overlays.Should().Equal("a.json", "b.json");
    }

    [Theory]
    [InlineData("\"dotnet-options\"", "project.validation must be an object")]
    [InlineData("{\"unknown\":1}", "Unknown project.validation setting")]
    [InlineData("{\"type\":\"typo\"}", "must be json-schema or dotnet-options")]
    [InlineData("{\"coverage\":\"typo\"}", "must be strict or registeredSections")]
    [InlineData("{\"overlays\":[\"  \"]}", "must not contain empty paths")]
    [InlineData("{\"type\":\"json-schema\",\"coverage\":\"strict\"}", "only supported by dotnet-options")]
    [InlineData("{\"type\":\"json-schema\",\"framework\":\"net10.0\"}", "only supported by dotnet-options")]
    [InlineData("{\"type\":\"json-schema\",\"overlays\":[\"a.json\"]}", "only supported by dotnet-options")]
    public void InvalidSettingsAreRejectedWithAnActionableMessage(string json, string expected)
    {
        Action parse = () => ValidationConfiguration.Parse(JsonNode.Parse(json));

        parse.Should().Throw<ArgumentException>().WithMessage($"*{expected}*");
    }

    [Fact]
    public void ChildSettingsOverrideInheritedOnesIndividually()
    {
        var parent = Parse("""{"type":"dotnet-options","coverage":"strict","framework":"net10.0"}""");
        var child = Parse("""{"coverage":"registeredSections"}""");

        var merged = parent.Merge(child);

        merged.Type.Should().Be(ValidationConfiguration.DotnetOptions);
        merged.Coverage.Should().Be(ValidationConfiguration.RegisteredSections);
        merged.Framework.Should().Be("net10.0");
    }

    [Fact]
    public void SwitchingProviderDiscardsInheritedProviderSettings()
    {
        var parent = Parse("""{"type":"dotnet-options","coverage":"strict","overlays":["a.json"]}""");

        var merged = parent.Merge(new ValidationConfiguration(ValidationConfiguration.JsonSchema));

        merged.Should().Be(new ValidationConfiguration(ValidationConfiguration.JsonSchema));
        merged.Coverage.Should().BeNull();
        merged.Overlays.Should().BeNull();
    }

    [Fact]
    public void MergingWithNothingKeepsTheInheritedSettings()
    {
        var parent = Parse("""{"type":"dotnet-options","coverage":"strict"}""");

        parent.Merge(null).Should().BeSameAs(parent);
    }

    [Fact]
    public void OverlaysParticipateInValueEquality()
    {
        var left = new ValidationConfiguration(Overlays: ["a.json"]);
        var right = new ValidationConfiguration(Overlays: ["a.json"]);
        var different = new ValidationConfiguration(Overlays: ["b.json"]);

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
        left.Should().NotBe(different);
        left.Should().NotBe(new ValidationConfiguration());
    }

    [Fact]
    public void SerializationUsesCamelCaseAndOmitsDerivedMembers()
    {
        var json = JsonSerializer.Serialize(
            Parse("""{"type":"dotnet-options","coverage":"strict"}"""),
            ValidationConfiguration.SerializerOptions);

        json.Should().Contain("\"type\"").And.Contain("\"coverage\"");
        json.Should().NotContain("EffectiveType").And.NotContain("effectiveType");
    }

    [Fact]
    public void ProjectSettingsSurviveParseMergeAndWrite()
    {
        var project = ProjectConfiguration.Parse(JsonNode.Parse("""
            {"validation":{"type":"dotnet-options","coverage":"strict"},"exportSchema":true,
             "environments":[{"name":"prod","validation":{"framework":"net10.0"},"exportSchema":false}]}
            """));

        var definition = ProjectDefinition.From(project.Merge(ProjectConfiguration.Parse(
            JsonNode.Parse("""{"name":"app"}"""))));

        definition.Validation!.Type.Should().Be(ValidationConfiguration.DotnetOptions);
        definition.ExportSchema.Should().BeTrue();

        var written = JsonNode.Parse(Write(definition.WriteTo))!;

        written["validation"]!["type"]!.GetValue<string>().Should().Be("dotnet-options");
        written["exportSchema"]!.GetValue<bool>().Should().BeTrue();
        written["environments"]!.AsArray()[0]!["validation"]!["framework"]!.GetValue<string>()
            .Should().Be("net10.0");
        written["environments"]!.AsArray()[0]!["exportSchema"]!.GetValue<bool>().Should().BeFalse();
    }

    [Fact]
    public void ProjectsWithoutValidationWriteNothingExtra()
    {
        var project = ProjectConfiguration.Parse(JsonNode.Parse("""{"name":"app"}"""));
        var definition = ProjectDefinition.From(project);

        var written = JsonNode.Parse(Write(definition.WriteTo))!.AsObject();

        written.Should().NotContainKey("validation").And.NotContainKey("exportSchema");
    }

    [Fact]
    public void EnvironmentsInheritAndOverrideIndependently()
    {
        var parent = EnvironmentConfiguration.Parse(JsonNode.Parse("""
            {"name":"prod","enabled":true,"validation":{"type":"dotnet-options","coverage":"strict"},
             "exportSchema":true}
            """)!);
        var child = EnvironmentConfiguration.Parse(JsonNode.Parse("""
            {"validation":{"coverage":"registeredSections"}}
            """)!);

        var merged = parent.Merge(child);

        merged.Name.Should().Be("prod");
        merged.Validation!.Coverage.Should().Be(ValidationConfiguration.RegisteredSections);
        merged.Validation.Type.Should().Be(ValidationConfiguration.DotnetOptions);
        merged.ExportSchema.Should().BeTrue();
    }

    [Fact]
    public void ShorthandStringEnvironmentsStillParse()
    {
        var environment = EnvironmentConfiguration.Parse(JsonNode.Parse("\"dev\"")!);

        environment.Name.Should().Be("dev");
        environment.Validation.Should().BeNull();
        environment.ExportSchema.Should().BeNull();
    }

    [Theory]
    [InlineData("{\"codeFirst\":{\"enabled\":true}}")]
    [InlineData("{\"name\":\"app\",\"codeFirst\":true}")]
    public void TheSupersededDraftKeyIsRejectedRatherThanIgnored(string json)
    {
        Action project = () => ProjectConfiguration.Parse(JsonNode.Parse(json));
        Action environment = () => EnvironmentConfiguration.Parse(JsonNode.Parse(json)!);

        project.Should().Throw<ArgumentException>().WithMessage("*validation*");
        environment.Should().Throw<ArgumentException>().WithMessage("*validation*");
    }

    private static ValidationConfiguration Parse(string json)
    {
        return ValidationConfiguration.Parse(JsonNode.Parse(json))!;
    }

    private static string Write(Action<Utf8JsonWriter> write)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            write(writer);
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }
}
