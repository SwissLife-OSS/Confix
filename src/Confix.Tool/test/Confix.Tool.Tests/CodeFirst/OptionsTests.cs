using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix.CodeFirst.Tests;

public sealed class OptionsTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"Mail\":null}")]
    [InlineData("{\"Mail\":{\"Host\":\"\"}}")]
    [InlineData("{\"Mail\":{\"Host\":\"  \"}}")]
    [InlineData("{\"Mail\":{\"Host\":\"server\",\"Port\":0}}")]
    [InlineData("{\"Mail\":{\"Host\":\"server\",\"Typo\":1}}")]
    [InlineData("{\"Mail\":{\"Host\":\"server\"},\"Typo\":{}}")]
    public void InvalidConfigurationFails(string json)
    {
        using var configuration = Config(json);
        using var provider = Services(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().NotBeEmpty();
    }

    [Fact]
    public void InitializersSupplyValuesButNotMissingSections()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"}}");
        using var provider = Services(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
        provider.GetRequiredService<IOptions<Mail>>().Value.Port.Should().Be(587);
        provider.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void RegisteredSectionsExplicitlyAllowsUnownedRoots()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"},\"Other\":{}}");
        using var provider = Services(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration, strict: false).Should().BeEmpty();
        ContractValidation.Validate(provider, configuration).Should()
            .Contain(e => e.Contains("Other"));
    }

    [Fact]
    public void StartupRejectsUnownedJsonButNotAmbientConfiguration()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"},\"Other\":1}");
        using var provider = Services(configuration).BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void RecursesThroughItemsAndRequiredKeys()
    {
        using var configuration = Config("{\"Nested\":{\"Items\":[{\"Host\":\" \"}],\"Explicit\":false}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Nested>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().NotBeEmpty();

        using var missing = Config("{\"Nested\":{\"Items\":[]}}");
        var other = new ServiceCollection();
        other.AddConfixOptions<Nested>(missing);
        using var otherProvider = other.BuildServiceProvider();

        ContractValidation.Validate(otherProvider, missing).Should().NotBeEmpty();
    }

    [Fact]
    public void CustomValidationCannotLeakValues()
    {
        using var configuration = Config("{\"Secret\":{\"Value\":\"sensitive-example\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Secret>(configuration);
        using var provider = services.BuildServiceProvider();

        var errors = ContractValidation.Validate(provider, configuration);

        errors.Should().NotBeEmpty();
        string.Join(" ", errors).Should().NotContain("sensitive-example");
    }

    [Fact]
    public void ValidationProviderOverridesMergeAndRejectTypos()
    {
        var parent = ValidationConfiguration.Parse(JsonNode.Parse(
            """{"type":"dotnet-options","coverage":"strict","framework":"net10.0"}"""))!;
        var child = ValidationConfiguration.Parse(JsonNode.Parse(
            """{"coverage":"registeredSections"}"""))!;

        parent.Merge(child).Should()
            .Be(new ValidationConfiguration("dotnet-options", "registeredSections", "net10.0"));
        parent.Merge(new ValidationConfiguration("json-schema")).Should()
            .Be(new ValidationConfiguration("json-schema"));
        new ValidationConfiguration().EffectiveType.Should().Be("json-schema");
    }

    [Fact]
    public void OverlaysAreComparedByValue()
    {
        var left = new ValidationConfiguration("dotnet-options", Overlays: ["a.json"]);
        var right = new ValidationConfiguration("dotnet-options", Overlays: ["a.json"]);

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Theory]
    [InlineData("{\"type\":\"typo\"}")]
    [InlineData("{\"enabled\":true}")]
    [InlineData("{\"exportSchema\":true}")]
    [InlineData("{\"coverage\":\"typo\"}")]
    [InlineData("{\"type\":\"json-schema\",\"framework\":\"net10.0\"}")]
    public void InvalidValidationSettingsAreRejected(string json)
    {
        Action parse = () => ValidationConfiguration.Parse(JsonNode.Parse(json));

        parse.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ProjectAndEnvironmentKeepExportSeparateFromValidation()
    {
        var parent = ProjectConfiguration.Parse(JsonNode.Parse("""
            {"validation":{"type":"dotnet-options","coverage":"strict"},"exportSchema":true,
             "environments":[{"name":"prod","validation":{"coverage":"registeredSections"},"exportSchema":false}]}
            """));
        var child = ProjectConfiguration.Parse(JsonNode.Parse("""{"exportSchema":false}"""));

        var merged = parent.Merge(child);

        merged.ExportSchema.Should().BeFalse();
        merged.Validation!.Type.Should().Be("dotnet-options");

        var environment = EnvironmentDefinition.From(merged.Environments!.Single());

        environment.ExportSchema.Should().BeFalse();
        merged.Validation.Merge(environment.Validation).Coverage.Should().Be("registeredSections");

        var json = JsonSerializer.Serialize(merged.Validation);

        json.Should().NotContain("EffectiveType").And.NotContain("ExportSchema");
    }

    [Fact]
    public void OptionalSectionsAreSkippedUntilPresent()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<OptionalMail>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
        provider.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void AbsentOptionalSectionDoesNotConstructOptionsAtStartup()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Absent>(configuration);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void NamedRegistrationsAndRuntimeOverridesUseNormalOptionsPipeline()
    {
        var json = "{\"First\":{\"Host\":\"one\"},\"Second\":{\"Host\":\"two\"}}";
        var overrides = new Dictionary<string, string?>
        {
            ["Second:Port"] = "70000",
            ["AMBIENT_KEY"] = "ignored"
        };

        using var configuration = (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .AddInMemoryCollection(overrides)
            .Build();

        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration, "First", "first");
        services.AddConfixOptions<Mail>(configuration, "Second", "second");
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptionsMonitor<Mail>>().Get("first").Host.Should().Be("one");

        Action second = () => provider.GetRequiredService<IOptionsMonitor<Mail>>().Get("second");

        second.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void ValidatorsCanUseExplicitServices()
    {
        using var configuration = Config("{\"ServiceChecked\":{\"Value\":\"configured\"}}");
        var services = new ServiceCollection();
        services.AddSingleton(new CheckState(false));
        services.AddConfixOptions<ServiceChecked>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .Contain(e => e.Contains("ServiceChecked:Value"));
    }

    [Fact]
    public void RejectsNullItemsAndWrongScalarShapes()
    {
        using var configuration = Config("{\"Collection\":{\"Items\":[null]}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Collection>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .Contain(e => e.Contains("Collection:Items:0: null items are not allowed."));

        using var invalid = Config("{\"Mail\":{\"Host\":\"valid\",\"Port\":{\"unexpected\":1}}}");
        using var other = Services(invalid).BuildServiceProvider();

        ContractValidation.Validate(other, invalid).Should()
            .Contain(e => e.Contains("expected a scalar configuration value"));
    }

    [Fact]
    public void OverlappingSectionsAreRejectedWithBothPaths()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration);

        Action nested = () => services.AddConfixOptions<Nested>(configuration, "Mail:Inner");

        nested.Should().Throw<InvalidOperationException>()
            .WithMessage("*Mail:Inner*Mail*");
    }

    private static ConfigurationRoot Config(string json)
    {
        return (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    private static ServiceCollection Services(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration);

        return services;
    }

    [ConfixSection("Mail")]
    public sealed class Mail
    {
        [Required]
        public string Host { get; set; } = "";

        [Range(1, 65535)]
        public int Port { get; set; } = 587;
    }

    [ConfixSection("Nested")]
    public sealed class Nested
    {
        public List<Mail> Items { get; set; } = [];

        [ConfixRequiredKey]
        public bool Explicit { get; set; }
    }

    [ConfixSection("Secret")]
    public sealed class Secret : IValidatableObject
    {
        public string Value { get; set; } = "";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield return new ValidationResult(Value, [nameof(Value)]);
        }
    }

    [ConfixSection("Absent", Required = false)]
    public sealed class Absent
    {
        public Absent()
        {
            throw new InvalidOperationException("Must not be constructed without configuration.");
        }
    }

    [ConfixSection("Optional", Required = false)]
    public sealed class OptionalMail
    {
        [Required]
        public string Host { get; set; } = "";
    }

    [ConfixSection("Collection")]
    public sealed class Collection
    {
        [ConfixRequiredItems]
        public List<string?> Items { get; set; } = [];
    }

    [ConfixSection("ServiceChecked")]
    public sealed class ServiceChecked
    {
        [Check]
        public string Value { get; set; } = "";
    }

    public sealed record CheckState(bool Allowed);

    public sealed class CheckAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var state = (CheckState)context.GetService(typeof(CheckState))!;

            return state.Allowed
                ? ValidationResult.Success
                : new ValidationResult("Not allowed", [context.MemberName!]);
        }
    }
}
