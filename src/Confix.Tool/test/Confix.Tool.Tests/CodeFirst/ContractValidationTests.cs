using System.ComponentModel.DataAnnotations;
using System.Text;
using Confix;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers shape, key and object validation independently of the CLI.</summary>
public sealed class ContractValidationTests
{
    [Fact]
    public void NoRegisteredContractsIsAnError()
    {
        using var configuration = Config("{}");
        using var provider = new ServiceCollection().BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Contain("No active Confix contracts");
    }

    [Fact]
    public void UnknownKeysAreReportedWithTheirFullPath()
    {
        var errors = Validate<Root>("{\"Root\":{\"Child\":{\"Name\":\"a\",\"Typo\":1}}}");

        errors.Should().ContainSingle().Which.Should().Be("Root:Child:Typo: unknown configuration key.");
    }

    [Fact]
    public void ConfigurationKeyNameIsHonouredForBindingAndUnknownKeys()
    {
        Validate<Renamed>("{\"Renamed\":{\"actual-name\":\"value\"}}").Should().BeEmpty();
        Validate<Renamed>("{\"Renamed\":{\"Mapped\":\"value\"}}").Should()
            .ContainSingle().Which.Should().Contain("unknown configuration key");
    }

    [Fact]
    public void KeysAreMatchedCaseInsensitively()
    {
        Validate<Root>("{\"root\":{\"child\":{\"name\":\"a\"}}}").Should().BeEmpty();
    }

    [Fact]
    public void ScalarWhereObjectExpectedIsRejected()
    {
        Validate<Root>("{\"Root\":{\"Child\":\"not-an-object\"}}").Should()
            .Contain(e => e.Contains("expected structured configuration"));
    }

    [Fact]
    public void ObjectWhereScalarExpectedIsRejected()
    {
        Validate<Root>("{\"Root\":{\"Child\":{\"Name\":{\"nested\":1}}}}").Should()
            .Contain(e => e.Contains("expected a scalar configuration value"));
    }

    [Fact]
    public void MissingRequiredSectionIsReported()
    {
        Validate<Root>("{}").Should().Contain(e => e.Contains("required section is missing"));
    }

    [Fact]
    public void RequiredKeyDistinguishesAbsenceFromDefaultValue()
    {
        Validate<Flags>("{\"Flags\":{}}").Should()
            .ContainSingle().Which.Should().Be("Flags:Explicit: required key is missing.");
        Validate<Flags>("{\"Flags\":{\"Explicit\":false}}").Should().BeEmpty();
    }

    [Fact]
    public void DictionaryValuesAreValidatedPerEntry()
    {
        Validate<Dictionaries>("{\"Dictionaries\":{\"Entries\":{\"first\":{\"Name\":\"\"}}}}").Should()
            .Contain(e => e.Contains("Dictionaries:Entries:first:Name"));
        Validate<Dictionaries>("{\"Dictionaries\":{\"Entries\":{\"first\":{\"Name\":\"ok\"}}}}").Should()
            .BeEmpty();
    }

    [Fact]
    public void CollectionItemsAreValidatedByIndex()
    {
        Validate<Collections>("{\"Collections\":{\"Items\":[{\"Name\":\"ok\"},{\"Name\":\"\"}]}}").Should()
            .Contain(e => e.Contains("Collections:Items:1:Name"));
    }

    [Fact]
    public void RequiredItemsRejectsNullScalarEntriesButAllowsAnEmptyCollection()
    {
        Validate<Collections>("{\"Collections\":{\"NonNull\":[\"a\",null]}}").Should()
            .Contain(e => e.Contains("Collections:NonNull:1: null items are not allowed."));
        Validate<Collections>("{\"Collections\":{\"NonNull\":[]}}").Should().BeEmpty();
        Validate<Collections>("{\"Collections\":{\"NonNull\":[\"a\"]}}").Should().BeEmpty();
    }

    [Fact]
    public void NullObjectItemsAreMaterializedByTheBinderAndCaughtByMemberRules()
    {
        // The binder creates an instance for a null object entry, so the item itself is never
        // null; its required members are what surface the problem.
        Validate<Collections>("{\"Collections\":{\"Items\":[null]}}").Should()
            .ContainSingle().Which.Should().Be("Collections:Items:0:Name: declared validation rule failed.");
    }

    [Fact]
    public void RequiredItemsAlsoChecksDictionaryValues()
    {
        Validate<Collections>("{\"Collections\":{\"Lookup\":{\"a\":null}}}").Should()
            .Contain(e => e.Contains("Collections:Lookup:0: null items are not allowed."));
    }

    [Fact]
    public void EmptyCollectionsAndDictionariesAreAccepted()
    {
        // An empty JSON array reaches IConfiguration as an empty value, not as a container.
        Validate<Collections>("{\"Collections\":{\"Items\":[],\"NonNull\":[],\"Lookup\":{}}}").Should()
            .BeEmpty();
    }

    [Fact]
    public void AnEmptyValueIsStillRejectedForNonCollectionMembers()
    {
        Validate<Root>("{\"Root\":{\"Child\":[]}}").Should()
            .Contain(e => e.Contains("expected structured configuration"));
    }

    [Fact]
    public void ValidatableObjectsAndCustomAttributesRun()
    {
        Validate<Root>("{\"Root\":{\"Child\":{\"Name\":\"reject\"}}}").Should()
            .Contain(e => e.Contains("Root:Child: declared validation rule failed."));
    }

    [Fact]
    public void ThrowingValidatorsAreContainedAsDiagnostics()
    {
        Validate<Throws>("{\"Throws\":{\"Value\":\"x\"}}").Should()
            .Contain(e => e.Contains("declared validator could not complete"));
    }

    [Fact]
    public void SelfReferencingGraphsDoNotRecurseForever()
    {
        using var configuration = Config("{\"Cycle\":{\"Name\":\"a\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Cycle>(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Cycle>>().Value;
        options.Self = options;

        var act = () => ContractValidation.Validate(provider, configuration);

        act.Should().NotThrow();
    }

    [Fact]
    public void StrictCoverageWalksIntoPartiallyOwnedContainers()
    {
        // Messaging is only a structural parent of Messaging:Smtp and needs no contract itself.
        using var configuration = Config("{\"Messaging\":{\"Smtp\":{\"Name\":\"a\"},\"Other\":{}}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mounted>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Be("Messaging:Other: no active Confix contract owns this section.");
    }

    [Fact]
    public void StructuralParentMustNotBeAScalar()
    {
        using var configuration = Config("{\"Messaging\":\"scalar\"}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mounted>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .Contain(e => e.Contains("Messaging: expected a configuration section container."));
    }

    [Fact]
    public void ARootContractDisablesCoverageAndOwnsEverything()
    {
        using var configuration = Config("{\"Name\":\"a\",\"Anything\":{}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<RootContract>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Contain("Anything: unknown configuration key");
    }

    [Fact]
    public void NonStrictCoverageStillValidatesRegisteredSections()
    {
        using var configuration = Config("{\"Root\":{\"Child\":{\"Name\":\"\"}},\"Unowned\":{}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Root>(configuration);
        using var provider = services.BuildServiceProvider();

        var errors = ContractValidation.Validate(provider, configuration, strict: false);

        errors.Should().NotContain(e => e.Contains("Unowned"));
        errors.Should().Contain(e => e.Contains("Root:Child:Name"));
    }

    private static IReadOnlyList<string> Validate<T>(string json) where T : class
    {
        using var configuration = Config(json);
        var services = new ServiceCollection();
        services.AddConfixOptions<T>(configuration);
        using var provider = services.BuildServiceProvider();

        return ContractValidation.Validate(provider, configuration);
    }

    private static ConfigurationRoot Config(string json)
    {
        return (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    [ConfixSection("Root")]
    public sealed class Root
    {
        public Child Child { get; set; } = new();
    }

    public sealed class Child : IValidatableObject
    {
        [Required]
        public string Name { get; set; } = "";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Name == "reject")
            {
                yield return new ValidationResult("rejected");
            }
        }
    }

    [ConfixSection("Renamed")]
    public sealed class Renamed
    {
        [ConfigurationKeyName("actual-name")]
        public string Mapped { get; set; } = "";
    }

    [ConfixSection("Flags")]
    public sealed class Flags
    {
        [ConfixRequiredKey]
        public bool Explicit { get; set; }
    }

    [ConfixSection("Dictionaries")]
    public sealed class Dictionaries
    {
        public Dictionary<string, Child> Entries { get; set; } = [];
    }

    [ConfixSection("Collections")]
    public sealed class Collections
    {
        public List<Child> Items { get; set; } = [];

        [ConfixRequiredItems]
        public List<string?> NonNull { get; set; } = [];

        [ConfixRequiredItems]
        public Dictionary<string, string?> Lookup { get; set; } = [];
    }

    [ConfixSection("Throws")]
    public sealed class Throws
    {
        [ThrowingCheck]
        public string Value { get; set; } = "";
    }

    public sealed class ThrowingCheckAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
            => throw new InvalidOperationException("validator is broken");
    }

    [ConfixSection("Cycle")]
    public sealed class Cycle
    {
        [Required]
        public string Name { get; set; } = "";

        public Cycle? Self { get; set; }
    }

    [ConfixSection("Messaging:Smtp")]
    public sealed class Mounted
    {
        [Required]
        public string Name { get; set; } = "";
    }

    [ConfixSection("")]
    public sealed class RootContract
    {
        [Required]
        public string Name { get; set; } = "";
    }
}
