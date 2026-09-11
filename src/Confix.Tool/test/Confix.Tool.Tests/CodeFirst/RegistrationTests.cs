using System.ComponentModel.DataAnnotations;
using System.Text;
using Confix;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers the guard rails of AddConfixOptions and AddConfixModule.</summary>
public sealed class RegistrationTests
{
    [Fact]
    public void OptionsWithoutConfixSectionAreRejected()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();

        Action register = () => services.AddConfixOptions<Undeclared>(configuration);

        register.Should().Throw<InvalidOperationException>().WithMessage("*requires ConfixSection*");
    }

    [Theory]
    [InlineData("A::B")]
    [InlineData(":A")]
    [InlineData("A:")]
    [InlineData("A: :B")]
    public void SectionsWithEmptySegmentsAreRejected(string section)
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();

        Action register = () => services.AddConfixOptions<Mail>(configuration, section);

        register.Should().Throw<InvalidOperationException>().WithMessage("*nonempty path segments*");
    }

    [Fact]
    public void TheSameTypeAndNameCannotBeRegisteredTwice()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration, "First");

        Action again = () => services.AddConfixOptions<Mail>(configuration, "Second");

        again.Should().Throw<InvalidOperationException>()
            .WithMessage("*already registered under the name*");
    }

    [Fact]
    public void DifferentNamesOnDisjointSectionsAreAllowed()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration, "First", "first");

        Action second = () => services.AddConfixOptions<Mail>(configuration, "Second", "second");

        second.Should().NotThrow();
    }

    [Theory]
    [InlineData("Mail", "Mail")]
    [InlineData("Mail", "mail")]
    [InlineData("Mail", "Mail:Inner")]
    [InlineData("Mail:Inner", "Mail")]
    public void OverlappingSectionsAreRejectedNamingBothPaths(string first, string second)
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration, first, "first");

        Action overlapping = () => services.AddConfixOptions<Other>(configuration, second, "second");

        overlapping.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain(first).And.Contain(second);
    }

    [Fact]
    public void ARootContractCannotBeCombinedWithMountedContracts()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration);

        Action root = () => services.AddConfixOptions<Other>(configuration, "");

        root.Should().Throw<InvalidOperationException>().WithMessage("*(root)*");
    }

    [Fact]
    public void CoverageValidationIsRegisteredOnlyOncePerCollection()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration, "First", "first");
        services.AddConfixOptions<Mail>(configuration, "Second", "second");
        services.AddConfixOptions<Other>(configuration, "Third");

        services.Count(d => d.ServiceType == typeof(CoverageSource)).Should().Be(1);
        services.Count(d => d.ServiceType == typeof(IValidateOptions<CoverageOptions>)).Should().Be(1);
    }

    [Fact]
    public void EveryContractIsExposedForTheRunner()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration, "First", "first");
        services.AddConfixOptions<Other>(configuration, "Second");
        using var provider = services.BuildServiceProvider();

        var contracts = provider.GetServices<IConfixContract>().ToArray();

        contracts.Should().HaveCount(2);
        contracts.Should().Contain(c => c.Section == "First" && c.Name == "first" && c.Required);
        contracts.Should().Contain(c => c.OptionsType == typeof(Other));
    }

    [Fact]
    public void OptionalContractsReportTheirRequiredFlag()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();
        services.AddConfixOptions<OptionalSection>(configuration);
        using var provider = services.BuildServiceProvider();

        provider.GetServices<IConfixContract>().Single().Required.Should().BeFalse();
    }

    [Fact]
    public void ModulesConfigureTheCollectionTheyAreGiven()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();

        services.AddConfixModule<Module>(configuration);

        using var provider = services.BuildServiceProvider();

        provider.GetServices<IConfixContract>().Should().ContainSingle();
        provider.GetRequiredService<IOptions<Mail>>().Value.Port.Should().Be(2525);
    }

    [Fact]
    public void BindingFailuresDoNotEchoConfiguredValues()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\",\"Port\":\"not-a-number\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration);
        using var provider = services.BuildServiceProvider();

        var errors = ContractValidation.Validate(provider, configuration);

        errors.Should().Contain(e => e.Contains("configuration binding failed"));
        string.Join(" ", errors).Should().NotContain("not-a-number");
    }

    [Fact]
    public void ContractsKeepUsingTheStandardOptionsPipeline()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration).PostConfigure(o => o.Host = "rewritten");
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<Mail>>().Value.Host.Should().Be("rewritten");
        provider.GetRequiredService<IOptionsSnapshot<Mail>>().Value.Host.Should().Be("rewritten");
        provider.GetRequiredService<IOptionsMonitor<Mail>>().CurrentValue.Host.Should().Be("rewritten");
    }

    private static ConfigurationRoot Config(string json)
    {
        return (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    public sealed class Undeclared;

    [ConfixSection("Mail")]
    public sealed class Mail
    {
        [Required]
        public string Host { get; set; } = "";

        [Range(1, 65535)]
        public int Port { get; set; } = 587;
    }

    [ConfixSection("Other")]
    public sealed class Other
    {
        public string Value { get; set; } = "";
    }

    [ConfixSection("Optional", Required = false)]
    public sealed class OptionalSection
    {
        public string Value { get; set; } = "";
    }

    public sealed class Module : IConfixModule
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfixOptions<Mail>(configuration).PostConfigure(o => o.Port = 2525);
        }
    }
}
