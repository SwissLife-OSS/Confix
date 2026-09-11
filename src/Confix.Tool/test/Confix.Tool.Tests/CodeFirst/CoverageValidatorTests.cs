using System.ComponentModel.DataAnnotations;
using System.Text;
using Confix;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix.CodeFirst.Tests;

/// <summary>Covers the startup-time root coverage check and its source boundary.</summary>
public sealed class CoverageValidatorTests
{
    [Fact]
    public void AmbientEnvironmentKeysAreNotTreatedAsApplicationConfiguration()
    {
        using var configuration = (ConfigurationRoot)new ConfigurationBuilder()
            .AddJsonStream(Stream("{\"Mail\":{\"Host\":\"server\"}}"))
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PATH"] = "/usr/bin",
                ["SOME_CI_VARIABLE"] = "1"
            })
            .Build();

        using var provider = Services(configuration).BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().NotThrow();
    }

    [Fact]
    public void UnownedJsonRootsFailStartup()
    {
        using var configuration = Json("{\"Mail\":{\"Host\":\"server\"},\"Rogue\":{\"a\":1}}");
        using var provider = Services(configuration).BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().Contain(f => f.Contains("Rogue"));
    }

    [Fact]
    public void StrictCoverageCanBeDisabledForGradualAdoption()
    {
        using var configuration = Json("{\"Mail\":{\"Host\":\"server\"},\"Rogue\":{\"a\":1}}");
        var services = Services(configuration);
        services.Configure<ConfixValidationSettings>(s => s.StrictCoverage = false);
        using var provider = services.BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().NotThrow();
    }

    [Fact]
    public void AnExplicitSourceBoundaryReplacesTheJsonProviderDefault()
    {
        using var configuration = Json("{\"Mail\":{\"Host\":\"server\"},\"Rogue\":{\"a\":1}}");
        using var boundary = (ConfigurationRoot)new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Mail:Host"] = "server" })
            .Build();

        var services = Services(configuration);
        services.Configure<ConfixValidationSettings>(s => s.Sources = boundary);
        using var provider = services.BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().NotThrow();
    }

    [Fact]
    public void AnExplicitSourceBoundaryStillReportsUnownedSections()
    {
        using var configuration = Json("{\"Mail\":{\"Host\":\"server\"}}");
        using var boundary = (ConfigurationRoot)new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Rogue:Key"] = "value" })
            .Build();

        var services = Services(configuration);
        services.Configure<ConfixValidationSettings>(s => s.Sources = boundary);
        using var provider = services.BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().Contain(f => f.Contains("Rogue"));
    }

    [Fact]
    public void ANonRootConfigurationNeedsAnExplicitSourceBoundary()
    {
        using var root = Json("{\"Mail\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(Wrap(root));
        using var provider = services.BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().Contain(f => f.Contains("IConfigurationRoot"));
    }

    [Fact]
    public void ARootContractSuppressesCoverageEntirely()
    {
        using var configuration = Json("{\"Host\":\"server\"}");
        var services = new ServiceCollection();
        services.AddConfixOptions<RootMail>(configuration);
        using var provider = services.BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().NotThrow();
    }

    [Fact]
    public void NestedOwnedSectionsAreWalkedRatherThanRejected()
    {
        using var configuration = Json("{\"Messaging\":{\"Smtp\":{\"Host\":\"server\"}}}");
        var services = new ServiceCollection();
        services.AddConfixOptions<MountedMail>(configuration);
        using var provider = services.BuildServiceProvider();

        Action start = () => provider.GetRequiredService<IStartupValidator>().Validate();

        start.Should().NotThrow();
    }

    /// <summary>Hides IConfigurationRoot so the validator cannot enumerate providers.</summary>
    private static IConfiguration Wrap(IConfiguration configuration)
    {
        return new WrappedConfiguration(configuration);
    }

    private static ServiceCollection Services(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddConfixOptions<Mail>(configuration);

        return services;
    }

    private static ConfigurationRoot Json(string json)
    {
        return (ConfigurationRoot)new ConfigurationBuilder().AddJsonStream(Stream(json)).Build();
    }

    private static MemoryStream Stream(string json)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }

    private sealed class WrappedConfiguration(IConfiguration inner) : IConfiguration
    {
        public string? this[string key]
        {
            get => inner[key];
            set => inner[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => inner.GetChildren();

        public Microsoft.Extensions.Primitives.IChangeToken GetReloadToken()
            => inner.GetReloadToken();

        public IConfigurationSection GetSection(string key) => inner.GetSection(key);
    }

    [ConfixSection("Mail")]
    public sealed class Mail
    {
        [Required]
        public string Host { get; set; } = "";
    }

    [ConfixSection("")]
    public sealed class RootMail
    {
        [Required]
        public string Host { get; set; } = "";
    }

    [ConfixSection("Messaging:Smtp")]
    public sealed class MountedMail
    {
        [Required]
        public string Host { get; set; } = "";
    }
}
