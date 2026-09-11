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
/// Covers contracts mounted inside another contract's section, mirroring the common .NET
/// layout where libraries contribute nested sections the parent type does not bind.
/// </summary>
public sealed class NestedContractTests
{
    [Fact]
    public void ParentAndNestedContractsValidateTheirOwnSubtrees()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvum":{"Url":"https://adsvum"},
             "Sonar":{"EnterpriseId":"e1"}}}
            """);
        using var provider = LuklaServices(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void NestedSectionsAreNotUnknownKeysOfTheParent()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvum":{"Url":"https://adsvum"},
             "Sonar":{"EnterpriseId":"e1"}}}
            """);
        using var provider = LuklaServices(configuration).BuildServiceProvider();

        var errors = ContractValidation.Validate(provider, configuration);

        errors.Should().NotContain(e => e.Contains("unknown configuration key"));
    }

    [Fact]
    public void TyposNextToNestedSectionsAreStillUnknownKeys()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvun":{"Url":"x"},
             "Adsvum":{"Url":"https://adsvum"},"Sonar":{"EnterpriseId":"e1"}}}
            """);
        using var provider = LuklaServices(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Be("Lukla:Adsvun: unknown configuration key.");
    }

    [Fact]
    public void NestedContractRulesAreEnforcedWithTheirFullPath()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvum":{"Url":""},
             "Sonar":{"EnterpriseId":"e1"}}}
            """);
        using var provider = LuklaServices(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Be("Lukla:Adsvum:Url: declared validation rule failed.");
    }

    [Fact]
    public void AMissingRequiredNestedSectionIsReportedByTheNestedContract()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Sonar":{"EnterpriseId":"e1"}}}
            """);
        using var provider = LuklaServices(configuration).BuildServiceProvider();

        var errors = ContractValidation.Validate(provider, configuration);

        errors.Should().Contain("Lukla:Adsvum: required section is missing.");
        errors.Should().NotContain(e => e.Contains("unknown configuration key"));
    }

    [Fact]
    public void OptionalNestedSectionsMayBeAbsent()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvum":{"Url":"https://adsvum"}}}
            """);
        using var provider = LuklaServices(configuration).BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void DeepNestingWalksUnboundStructuralContainers()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvum":{"Url":"https://adsvum"},
             "Integrations":{"Email":{"Sender":"a@b.c"}}}}
            """);
        var services = LuklaServices(configuration);
        services.AddConfixOptions<EmailOptions>(configuration, "Lukla:Integrations:Email");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void StrayKeysInsideStructuralContainersAreUnknown()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvum":{"Url":"https://adsvum"},
             "Integrations":{"Email":{"Sender":"a@b.c"},"Stray":{"a":1}}}}
            """);
        var services = LuklaServices(configuration);
        services.AddConfixOptions<EmailOptions>(configuration, "Lukla:Integrations:Email");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Be("Lukla:Integrations:Stray: unknown configuration key.");
    }

    [Fact]
    public void AScalarWhereAStructuralContainerIsExpectedIsRejected()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvum":{"Url":"https://adsvum"},
             "Integrations":"scalar"}}
            """);
        var services = LuklaServices(configuration);
        services.AddConfixOptions<EmailOptions>(configuration, "Lukla:Integrations:Email");
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .Contain(e => e.Contains("Lukla:Integrations: expected a configuration section container."));
    }

    [Fact]
    public void StartupValidationAcceptsNestedContracts()
    {
        using var configuration = Config("""
            {"Lukla":{"ProjectName":"lukla","Adsvum":{"Url":"https://adsvum"},
             "Sonar":{"EnterpriseId":"e1"}}}
            """);
        using var provider = LuklaServices(configuration).BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().NotThrow();
        provider.GetRequiredService<IOptions<AdsvumOptions>>().Value.Url.Should().Be("https://adsvum");
    }

    [Fact]
    public void ARootContractCanHostNestedContracts()
    {
        using var configuration = Config("""
            {"Name":"app","Mail":{"Host":"server"}}
            """);
        var services = new ServiceCollection();
        services.AddConfixOptions<RootOptions>(configuration);
        services.AddConfixOptions<MailOptions>(configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should().BeEmpty();
    }

    [Fact]
    public void ValidationReportsConflictsForContractsRegisteredWithoutTheGuard()
    {
        // The runner receives contracts through the generated catalog; a hand-built collection
        // can bypass AddConfixOptions, so Validate re-checks the invariant.
        using var configuration = Config("{\"Lukla\":{\"ProjectName\":\"x\"}}");
        var services = new ServiceCollection();
        services.AddSingleton<IConfixContract>(new FakeContract("Lukla", typeof(ParentOptions)));
        services.AddSingleton<IConfixContract>(
            new FakeContract("Lukla:ProjectName", typeof(MailOptions)));
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .Contain(e => e.Contains("'ProjectName' is bound by ParentOptions"));
    }

    [Fact]
    public void SchemasGraftNestedContractsIntoTheParent()
    {
        using var configuration = Config("{}");
        using var provider = LuklaServices(configuration).BuildServiceProvider();

        var schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), strict: true);

        var parent = schema["properties"]!["Lukla"]!;

        parent["properties"]!["ProjectName"].Should().NotBeNull();
        parent["properties"]!["Adsvum"]!["properties"]!["Url"].Should().NotBeNull();
        parent["properties"]!["Sonar"].Should().NotBeNull();

        var required = parent["required"]!.AsArray().Select(n => n!.GetValue<string>()).ToArray();

        required.Should().Contain("Adsvum").And.NotContain("Sonar");
    }

    [Fact]
    public void SchemaGraftingIsIndependentOfRegistrationOrder()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<AdsvumOptions>(configuration);
        services.AddConfixOptions<ParentOptions>(configuration);
        using var provider = services.BuildServiceProvider();

        var schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), strict: true);

        schema["properties"]!["Lukla"]!["properties"]!["Adsvum"].Should().NotBeNull();
    }

    [Fact]
    public void RootContractSchemasReceiveGraftsToo()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<RootOptions>(configuration);
        services.AddConfixOptions<MailOptions>(configuration);
        using var provider = services.BuildServiceProvider();

        var schema = SchemaExport.Export(provider.GetServices<IConfixContract>(), strict: true);

        schema["properties"]!["Name"].Should().NotBeNull();
        schema["properties"]!["Mail"]!["properties"]!["Host"].Should().NotBeNull();
        schema["required"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Contain("Mail");
    }

    private static ServiceCollection LuklaServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddConfixOptions<ParentOptions>(configuration);
        services.AddConfixOptions<AdsvumOptions>(configuration);
        services.AddConfixOptions<SonarOptions>(configuration);

        return services;
    }

    private static ConfigurationRoot Config(string json)
    {
        return (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    [ConfixSection("Lukla")]
    public sealed class ParentOptions
    {
        [Required]
        public string ProjectName { get; set; } = "";
    }

    [ConfixSection("Lukla:Adsvum")]
    public sealed class AdsvumOptions
    {
        [Required]
        [Url]
        public string Url { get; set; } = "";
    }

    [ConfixSection("Lukla:Sonar", Required = false)]
    public sealed class SonarOptions
    {
        [Required]
        public string EnterpriseId { get; set; } = "";
    }

    [ConfixSection("Lukla:Integrations:Email")]
    public sealed class EmailOptions
    {
        [Required]
        public string Sender { get; set; } = "";
    }

    [ConfixSection("")]
    public sealed class RootOptions
    {
        [Required]
        public string Name { get; set; } = "";
    }

    [ConfixSection("Mail")]
    public sealed class MailOptions
    {
        [Required]
        public string Host { get; set; } = "";
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
