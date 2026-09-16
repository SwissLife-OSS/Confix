using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Nodes;
using Confix;
using Confix.Runner;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers the advisory IDE schema produced for code-first projects.</summary>
public sealed class SchemaExportTests
{
    [Fact]
    public void ContractsAreMountedUnderTheirSectionPath()
    {
        var schema = Export<Mounted>();

        schema["$schema"]!.GetValue<string>().Should().Contain("json-schema.org");
        schema["properties"]!["Messaging"]!["properties"]!["Smtp"].Should().NotBeNull();
        schema["required"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Contain("Messaging");
    }

    [Fact]
    public void OptionalContractsAreNotMarkedRequired()
    {
        var schema = Export<OptionalSection>();

        schema["required"]!.AsArray().Should().BeEmpty();
        schema["properties"]!["Optional"].Should().NotBeNull();
    }

    [Fact]
    public void StrictCoverageForbidsAdditionalRootProperties()
    {
        Export<Simple>()["additionalProperties"]!.GetValue<bool>().Should().BeFalse();
        Export<Simple>(strict: false)["additionalProperties"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void ARootContractReplacesTheGeneratedRoot()
    {
        var schema = Export<RootContract>();

        schema["$schema"]!.GetValue<string>().Should().Contain("json-schema.org");
        schema["properties"]!["Host"].Should().NotBeNull();
        schema["properties"]!.AsObject().Should().NotContainKey("");
    }

    [Fact]
    public void EveryValueAlsoAcceptsAConfixVariableExpression()
    {
        var host = Member<Simple>("Simple", "Host");

        var alternatives = host["anyOf"]!.AsArray();

        alternatives.Should().HaveCount(2);
        alternatives[1]!["pattern"]!.GetValue<string>().Should().Be("^\\$[^:]+:.+");
    }

    [Fact]
    public void RequiredStringsRejectWhitespaceOnlyValues()
    {
        var host = Member<Simple>("Simple", "Host")["anyOf"]![0]!;

        host["type"]!.GetValue<string>().Should().Be("string");
        host["minLength"]!.GetValue<int>().Should().Be(1);
        host["pattern"]!.GetValue<string>().Should().Be("\\S");
    }

    [Fact]
    public void IntegerRangesAreTranslated()
    {
        var port = Member<Simple>("Simple", "Port")["anyOf"]![0]!;

        port["minimum"]!.GetValue<int>().Should().Be(1);
        port["maximum"]!.GetValue<int>().Should().Be(65535);
    }

    [Fact]
    public void StringAndCollectionLengthsUseTheMatchingKeywords()
    {
        var name = Member<Lengths>("Lengths", "Name")["anyOf"]![0]!;
        var items = Member<Lengths>("Lengths", "Items")["anyOf"]![0]!;
        var bounded = Member<Lengths>("Lengths", "Bounded")["anyOf"]![0]!;

        name["minLength"]!.GetValue<int>().Should().Be(2);
        items["minItems"]!.GetValue<int>().Should().Be(1);
        items["maxItems"]!.GetValue<int>().Should().Be(3);
        bounded["minLength"]!.GetValue<int>().Should().Be(1);
        bounded["maxLength"]!.GetValue<int>().Should().Be(8);
    }

    [Fact]
    public void DescriptionsAreCarriedOver()
    {
        Member<Lengths>("Lengths", "Name")["anyOf"]![0]!["description"]!.GetValue<string>()
            .Should().Be("The display name.");
    }

    [Fact]
    public void ConfigurationKeyNamesAreUsedInsteadOfPropertyNames()
    {
        var properties = Section<Renamed>("Renamed")["properties"]!.AsObject();

        properties.Should().ContainKey("actual-name");
        properties.Should().NotContainKey("Mapped");
    }

    [Fact]
    public void JsonSerializationAttributesAreIgnoredBecauseBindingIgnoresThem()
    {
        var properties = Section<JsonAttributed>("JsonAttributed")["properties"]!.AsObject();

        properties.Should().ContainKey("Ignored").And.ContainKey("Renamed");
        properties.Should().NotContainKey("json_name");
    }

    [Fact]
    public void RequiredKeysBecomeSchemaRequirements()
    {
        var section = Section<Keys>("Keys");

        section["required"]!.AsArray().Select(n => n!.GetValue<string>()).Should()
            .Contain("Explicit").And.NotContain("Optional");
    }

    [Fact]
    public void UnknownPropertiesAreForbiddenInsideContracts()
    {
        Section<Simple>("Simple")["additionalProperties"]!.GetValue<bool>().Should().BeFalse();
    }

    [Fact]
    public void ReferencesStayResolvableAfterMounting()
    {
        AllReferencesResolve(Export<Recursive>());
    }

    [Fact]
    public void ReferencesStayResolvableInARootContract()
    {
        AllReferencesResolve(Export<RecursiveRoot>());
    }

    private static void AllReferencesResolve(JsonObject schema)
    {
        var references = new List<string>();

        Collect(schema, references);

        references.Should().NotBeEmpty();

        foreach (var reference in references)
        {
            Resolve(schema, reference).Should().NotBeNull($"'{reference}' must resolve");
        }
    }

    [Fact]
    public void MultipleContractsShareOneRoot()
    {
        using var configuration = Config();
        var services = new ServiceCollection();
        services.AddConfixOptions<Mounted>(configuration);
        services.AddConfixOptions<Simple>(configuration);
        using var provider = services.BuildServiceProvider();

        var schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), true);

        schema["properties"]!["Messaging"]!["properties"]!["Smtp"].Should().NotBeNull();
        schema["properties"]!["Simple"].Should().NotBeNull();
    }

    private static JsonNode Member<T>(string section, string property) where T : class
    {
        return Section<T>(section)["properties"]![property]!;
    }

    private static JsonNode Section<T>(string section) where T : class
    {
        return Export<T>()["properties"]![section]!;
    }

    private static JsonObject Export<T>(bool strict = true) where T : class
    {
        using var configuration = Config();
        var services = new ServiceCollection();
        services.AddConfixOptions<T>(configuration);
        using var provider = services.BuildServiceProvider();

        return SchemaExport.Export(provider.GetServices<IConfixContract>(), strict);
    }

    private static ConfigurationRoot Config()
    {
        return (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("{}")))
            .Build();
    }

    private static void Collect(JsonNode? node, List<string> references)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj["$ref"]?.GetValue<string>() is { } reference)
                {
                    references.Add(reference);
                }

                foreach (var property in obj)
                {
                    Collect(property.Value, references);
                }

                break;

            case JsonArray array:
                foreach (var item in array)
                {
                    Collect(item, references);
                }

                break;
        }
    }

    private static JsonNode? Resolve(JsonNode root, string reference)
    {
        var current = root;

        foreach (var segment in reference.Split('/').Skip(1))
        {
            var key = segment.Replace("~1", "/").Replace("~0", "~");

            current = current switch
            {
                JsonArray array when int.TryParse(key, out var index) && index < array.Count
                    => array[index],
                JsonObject obj => obj[key],
                _ => null
            };

            if (current is null)
            {
                return null;
            }
        }

        return current;
    }

    [ConfixSection("Simple")]
    public sealed class Simple
    {
        [Required]
        public string Host { get; set; } = "";

        [Range(1, 65535)]
        public int Port { get; set; } = 587;
    }

    [ConfixSection("Messaging:Smtp")]
    public sealed class Mounted
    {
        [Required]
        public string Host { get; set; } = "";
    }

    [ConfixSection("Optional", Required = false)]
    public sealed class OptionalSection
    {
        public string Value { get; set; } = "";
    }

    [ConfixSection("")]
    public sealed class RootContract
    {
        [Required]
        public string Host { get; set; } = "";
    }

    [ConfixSection("Lengths")]
    public sealed class Lengths
    {
        [Description("The display name.")]
        [MinLength(2)]
        public string Name { get; set; } = "";

        [MinLength(1)]
        [MaxLength(3)]
        public List<string> Items { get; set; } = [];

        [StringLength(8, MinimumLength = 1)]
        public string Bounded { get; set; } = "";
    }

    [ConfixSection("Renamed")]
    public sealed class Renamed
    {
        [ConfigurationKeyName("actual-name")]
        public string Mapped { get; set; } = "";
    }

    [ConfixSection("JsonAttributed")]
    public sealed class JsonAttributed
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public string Ignored { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("json_name")]
        public string Renamed { get; set; } = "";
    }

    [ConfixSection("Keys")]
    public sealed class Keys
    {
        [ConfixRequiredKey]
        public bool Explicit { get; set; }

        public bool Optional { get; set; }
    }

    [ConfixSection("Recursive")]
    public sealed class Recursive
    {
        public Node Root { get; set; } = new();
    }

    [ConfixSection("")]
    public sealed class RecursiveRoot
    {
        public Node Root { get; set; } = new();
    }

    public sealed class Node
    {
        public string Name { get; set; } = "";

        public List<Node> Children { get; set; } = [];
    }
}
