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
/// Covers sibling contracts that share a namespace section, the layout the .NET options
/// documentation uses when several components contribute settings under a common prefix.
/// No type is bound to the namespace itself.
/// </summary>
public sealed class NamespaceSectionTests
{
    [Fact]
    public void SiblingContractsValidateTheirOwnSubtrees()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Billing":{"Url":"https://billing"},
             "Search":{"EnterpriseId":"e1"}}}
            """);
        using var provider = PortalServices(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void TyposInsideTheNamespaceAreReported()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Billinh":{"Url":"x"},
             "Billing":{"Url":"https://billing"},"Search":{"EnterpriseId":"e1"}}}
            """);
        using var provider = PortalServices(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should()
            .Be("Portal:Billinh: no active Confix contract owns this section.");
    }

    [Fact]
    public void ContractRulesAreEnforcedWithTheirFullPath()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Billing":{"Url":""},
             "Search":{"EnterpriseId":"e1"}}}
            """);
        using var provider = PortalServices(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should()
            .Be("Portal:Billing:Url: declared validation rule failed.");
    }

    [Fact]
    public void AMissingRequiredSectionIsReportedByItsOwnContract()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Search":{"EnterpriseId":"e1"}}}
            """);
        using var provider = PortalServices(configuration).BuildServiceProvider();

        var errors = ContractValidation.Validate(provider, configuration);

        errors.Should().Contain("Portal:Billing: required section is missing.");
        errors.Should().NotContain(e => e.Contains("unknown configuration key"));
    }

    [Fact]
    public void OptionalSectionsMayBeAbsent()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Billing":{"Url":"https://billing"}}}
            """);
        using var provider = PortalServices(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void DeepNamespacesAreWalked()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Billing":{"Url":"https://billing"},
             "Integrations":{"Email":{"Sender":"a@b.c"}}}}
            """);
        var services = PortalServices(configuration);
        services.AddConfixOptions<EmailOptions>(configuration, "Portal:Integrations:Email");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void StrayKeysInsideNamespacesAreReported()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Billing":{"Url":"https://billing"},
             "Integrations":{"Email":{"Sender":"a@b.c"},"Stray":{"a":1}}}}
            """);
        var services = PortalServices(configuration);
        services.AddConfixOptions<EmailOptions>(configuration, "Portal:Integrations:Email");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should()
            .Be("Portal:Integrations:Stray: no active Confix contract owns this section.");
    }

    [Fact]
    public void AScalarWhereANamespaceIsExpectedIsRejected()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Billing":{"Url":"https://billing"},
             "Integrations":"scalar"}}
            """);
        var services = PortalServices(configuration);
        services.AddConfixOptions<EmailOptions>(configuration, "Portal:Integrations:Email");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .Contain(e => e.Contains("Portal:Integrations: expected a configuration section container."));
    }

    [Fact]
    public void StartupValidationAcceptsNamespacedContracts()
    {
        using var configuration = Config("""
            {"Portal":{"Core":{"ProjectName":"portal"},"Billing":{"Url":"https://billing"},
             "Search":{"EnterpriseId":"e1"}}}
            """);
        using var provider = PortalServices(configuration).BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().NotThrow();
        provider.GetRequiredService<IOptions<BillingOptions>>().Value.Url
            .Should().Be("https://billing");
    }

    [Fact]
    public void ValidationReportsNestingForContractsRegisteredWithoutTheGuard()
    {
        // The runner receives contracts through the generated catalog; a hand-built collection
        // can bypass AddConfixOptions, so Validate re-checks the invariant.
        using var configuration = Config("{\"Portal\":{\"ProjectName\":\"x\"}}");
        var services = new ServiceCollection();
        services.AddSingleton<IConfixContract>(new FakeContract("Portal", typeof(ParentOptions)));
        services.AddSingleton<IConfixContract>(
            new FakeContract("Portal:ProjectName", typeof(BillingOptions)));
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .Contain(e => e.Contains("cannot be nested inside 'Portal' of ParentOptions"));
    }

    [Fact]
    public void SchemasMountSiblingsUnderANamespaceNode()
    {
        using var configuration = Config("{}");
        using var provider = PortalServices(configuration).BuildServiceProvider();

        var schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), strict: true);

        var portal = schema["properties"]!["Portal"]!;

        portal["properties"]!["Core"]!["properties"]!["ProjectName"].Should().NotBeNull();
        portal["properties"]!["Billing"]!["properties"]!["Url"].Should().NotBeNull();
        portal["properties"]!["Search"].Should().NotBeNull();

        var required = portal["required"]!.AsArray().Select(n => n!.GetValue<string>()).ToArray();

        required.Should().Contain("Billing").And.NotContain("Search");
    }

    [Fact]
    public void SchemaMountingIsIndependentOfRegistrationOrder()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<BillingOptions>(configuration);
        services.AddConfixOptions<ParentOptions>(configuration);
        using var provider = services.BuildServiceProvider();

        var schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), strict: true);

        var portal = schema["properties"]!["Portal"]!;

        portal["properties"]!["Billing"].Should().NotBeNull();
        portal["properties"]!["Core"].Should().NotBeNull();
    }

    private static ServiceCollection PortalServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddConfixOptions<ParentOptions>(configuration);
        services.AddConfixOptions<BillingOptions>(configuration);
        services.AddConfixOptions<SearchOptions>(configuration);

        return services;
    }

    private static ConfigurationRoot Config(string json)
    {
        return (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    [ConfixSection("Portal:Core")]
    public sealed class ParentOptions
    {
        [Required]
        public string ProjectName { get; set; } = "";
    }

    [ConfixSection("Portal:Billing")]
    public sealed class BillingOptions
    {
        [Required]
        [Url]
        public string Url { get; set; } = "";
    }

    [ConfixSection("Portal:Search", Required = false)]
    public sealed class SearchOptions
    {
        [Required]
        public string EnterpriseId { get; set; } = "";
    }

    [ConfixSection("Portal:Integrations:Email")]
    public sealed class EmailOptions
    {
        [Required]
        public string Sender { get; set; } = "";
    }

    private sealed record FakeContract(string Section, Type OptionsType) : IConfixContract
    {
        public string Name => "";

        public bool Required => false;

        public void Validate(IServiceProvider services)
        {
        }
    }
}
