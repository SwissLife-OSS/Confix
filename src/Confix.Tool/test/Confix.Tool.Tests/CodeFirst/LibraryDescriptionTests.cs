using System.ComponentModel.DataAnnotations;
using System.Text;
using Confix;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix.CodeFirst.Tests;

/// <summary>
/// A library describes its section unconditionally; enforcement depends on whether the
/// application itself uses Confix, or whether confix is inspecting the application.
/// </summary>
public sealed class LibraryDescriptionTests : IDisposable
{
    public void Dispose()
    {
        Environment.SetEnvironmentVariable("CONFIX_VALIDATION", null);
    }

    [Fact]
    public void AnApplicationWithoutConfixIsUnaffectedByABadValue()
    {
        using var configuration = Config("""{"Mail":{"Hst":"typo"}}""");
        var services = new ServiceCollection();
        RegisterLikeALibrary(services, configuration);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<Mail>>().Value.Host.Should().Be("");
    }

    [Fact]
    public void AnApplicationUsingConfixEnforcesLibraryDescriptions()
    {
        using var configuration = Config("""{"Mail":{"Hst":"typo"},"App":{"Name":"x"}}""");
        var services = new ServiceCollection();
        RegisterLikeALibrary(services, configuration);
        services.AddConfixOptions<AppOptions>(configuration);
        using var provider = services.BuildServiceProvider();

        Action resolve = () => _ = provider.GetRequiredService<IOptions<Mail>>().Value;

        resolve.Should().Throw<ConfixValidationException>().Which.Message.Should().Contain("Hst");
    }

    [Fact]
    public void ValidationRunsEnforceLibraryDescriptionsWithoutAnyApplicationOptIn()
    {
        Environment.SetEnvironmentVariable("CONFIX_VALIDATION", "true");
        using var configuration = Config("""{"Mail":{"Hst":"typo"}}""");
        var services = new ServiceCollection();
        RegisterLikeALibrary(services, configuration);
        using var provider = services.BuildServiceProvider();

        ContractValidation.Validate(provider, configuration).Should()
            .ContainSingle().Which.Should().Contain("Hst");
    }

    [Fact]
    public void AnOverlappingDescriptionIsReportedInsteadOfCrashingTheHost()
    {
        using var configuration = Config("""{"Mail":{"Host":"server"}}""");
        var services = new ServiceCollection();
        RegisterLikeALibrary(services, configuration);

        // A second library describing the same section must not take the application down.
        Action second = () => services.AddConfixSection<Other>(configuration, "Mail");

        second.Should().NotThrow();

        using var provider = services.BuildServiceProvider();
        ContractValidation.Validate(provider, configuration).Should()
            .Contain(error => error.Contains("already claimed"));
    }

    private static void RegisterLikeALibrary(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConfiguration>(configuration);
        services.AddOptions<Mail>().BindConfiguration("Mail");
        services.AddConfixSection<Mail>(configuration);
    }

    private static ConfigurationRoot Config(string json)
    {
        return (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();
    }

    [ConfixSection("Mail")]
    public sealed class Mail
    {
        public string Host { get; set; } = "";
    }

    [ConfixSection("App")]
    public sealed class AppOptions
    {
        [Required]
        public string Name { get; set; } = "";
    }

    public sealed class Other
    {
        public string Value { get; set; } = "";
    }
}
