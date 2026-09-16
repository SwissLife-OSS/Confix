using System.ComponentModel.DataAnnotations;
using System.Text;
using Confix;
using Confix.Runner;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix.CodeFirst.Tests;

/// <summary>
/// Covers ownership of sections whose types cannot be annotated and of sections owned by
/// another component entirely.
/// </summary>
public sealed class ExternalSectionTests
{
    [Fact]
    public void AnUnannotatedTypeCanBeMountedAtAnExplicitSection()
    {
        using var configuration = Config("{\"MongoDb\":{\"ConnectionString\":\"mongodb://x\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<ExternalOptions>(configuration, "MongoDb");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
        provider.GetRequiredService<IOptions<ExternalOptions>>().Value.ConnectionString
            .Should().Be("mongodb://x");
    }

    [Fact]
    public void AnUnannotatedTypeStillValidatesItsOwnShapeAndRules()
    {
        using var configuration = Config("{\"MongoDb\":{\"Typo\":1}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<ExternalOptions>(configuration, "MongoDb");
        using var provider = services.BuildServiceProvider();

        var errors = ContractValidation.Validate(provider, configuration);

        errors.Should().Contain(e => e.Contains("MongoDb:Typo: unknown configuration key."));
        errors.Should().Contain(e => e.Contains("MongoDb:ConnectionString"));
    }

    [Fact]
    public void AnUnannotatedTypeWithoutASectionIsRejected()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();

        Action register = () => services.AddConfixOptions<ExternalOptions>(configuration);

        register.Should().Throw<InvalidOperationException>()
            .WithMessage("*no ConfixSection attribute*section must be supplied*");
    }

    [Fact]
    public void AnExplicitSectionOverridesTheAttribute()
    {
        using var configuration = Config("{\"Elsewhere\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration, "Elsewhere");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void AnUnannotatedTypeIsRequiredByDefault()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<ExternalOptions>(configuration, "MongoDb");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .Contain("MongoDb: required section is missing.");
    }

    [Fact]
    public void RequirednessCanBeOverriddenAtTheRegistrationSite()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<ExternalOptions>(configuration, "MongoDb", required: false);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
        provider.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void AClaimedSectionCountsTowardsCoverageWithoutInspectingIt()
    {
        using var configuration = Config("""
            {"Mail":{"Host":"server"},"Logging":{"LogLevel":{"Default":"Information"}}}
            """);
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration);
        services.AddConfixSection(configuration, "Logging");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void WithoutTheClaimStrictCoverageStillReportsTheSection()
    {
        using var configuration = Config("""
            {"Mail":{"Host":"server"},"Logging":{"LogLevel":{"Default":"Information"}}}
            """);
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Contain("Logging");
    }

    [Fact]
    public void ARequiredClaimReportsAnAbsentSection()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration);
        services.AddConfixSection(configuration, "Security");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Be("Security: required section is missing.");
    }

    [Fact]
    public void AnOptionalClaimToleratesAnAbsentSection()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration);
        services.AddConfixSection(configuration, "Security", required: false);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void ClaimsAreOpaqueSoNothingMayBeNestedInside()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixSection(configuration, "Security");

        Action nested = () => services.AddConfixOptions<Annotated>(configuration, "Security:Mail");

        nested.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("cannot be nested inside claimed section 'Security'");
    }

    [Fact]
    public void AClaimCannotShadowAnExistingContract()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration);

        Action claim = () => services.AddConfixSection(configuration, "Mail");

        claim.Should().Throw<InvalidOperationException>().WithMessage("*already claimed*");
    }

    [Fact]
    public void ClaimsCannotBeNestedInsideAContract()
    {
        using var configuration = Config("""
            {"Portal":{"Host":"server","Telemetry":{"Anything":true}}}
            """);
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration, "Portal");

        Action claim = () => services.AddConfixSection(configuration, "Portal:Telemetry");

        claim.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("cannot be nested inside 'Portal' of Annotated");
    }

    [Fact]
    public void StartupValidationHonoursClaims()
    {
        using var configuration = Config("""
            {"Mail":{"Host":"server"},"Logging":{"LogLevel":{"Default":"Information"}}}
            """);
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration);
        services.AddConfixSection(configuration, "Logging");
        using var provider = services.BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().NotThrow();
    }

    [Fact]
    public void ClaimedSectionsAreExportedAsPermissiveObjects()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Annotated>(configuration);
        services.AddConfixSection(configuration, "Logging");
        using var provider = services.BuildServiceProvider();

        var schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), strict: true);
        var logging = schema["properties"]!["Logging"]!;

        logging["type"]!.GetValue<string>().Should().Be("object");
        logging.AsObject().Should().NotContainKey("additionalProperties");
        schema["properties"]!["Mail"]!["properties"]!["Host"].Should().NotBeNull();
    }

    private static ConfigurationRoot Config(string json)
    {
        return (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    /// <summary>Stands in for a type owned by another package that cannot be annotated.</summary>
    public sealed class ExternalOptions
    {
        [Required]
        public string ConnectionString { get; set; } = "";
    }

    [ConfixSection("Mail")]
    public sealed class Annotated
    {
        [Required]
        public string Host { get; set; } = "";
    }
}
