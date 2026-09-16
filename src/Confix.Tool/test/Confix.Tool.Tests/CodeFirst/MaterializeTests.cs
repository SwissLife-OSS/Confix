using System.ComponentModel.DataAnnotations;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix.CodeFirst.Tests;

/// <summary>
/// Covers the opt-in composition-time read. Registration alone must stay lazy; materializing
/// is an explicit call and is validated like every other read.
/// </summary>
public sealed class MaterializeTests
{
    [Fact]
    public void RegistrationAloneDoesNotBind()
    {
        // An invalid value would throw during binding, so reaching here proves laziness.
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\",\"Port\":\"nope\"}}");
        var services = new ServiceCollection();

        Action register = () => services.AddConfixOptions<Mail>(configuration);

        register.Should().NotThrow();
    }

    [Fact]
    public void MaterializeReturnsTheBoundValue()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\",\"Port\":2525}}");
        var services = new ServiceCollection();

        var value = services.AddConfixOptions<Mail>(configuration).Materialize();

        value.Host.Should().Be("server");
        value.Port.Should().Be(2525);
    }

    [Fact]
    public void MaterializeStillRegistersTheOptionsPipeline()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();

        services.AddConfixOptions<Mail>(configuration).Materialize();

        using var provider = services.BuildServiceProvider();

        provider.GetServices<IConfixContract>().Should().ContainSingle();
        provider.GetRequiredService<IOptions<Mail>>().Value.Host.Should().Be("server");
    }

    [Fact]
    public void MaterializeValidatesDeclaredRules()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"\"}}");
        var services = new ServiceCollection();

        Action materialize = () => services.AddConfixOptions<Mail>(configuration).Materialize();

        materialize.Should().Throw<ConfixValidationException>()
            .Which.Failures.Should().ContainSingle()
            .Which.Should().Contain("Mail:Host");
    }

    [Fact]
    public void MaterializeRejectsUnknownKeys()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\",\"Typo\":1}}");
        var services = new ServiceCollection();

        Action materialize = () => services.AddConfixOptions<Mail>(configuration).Materialize();

        materialize.Should().Throw<ConfixValidationException>()
            .Which.Failures.Should().ContainSingle()
            .Which.Should().Be("Mail:Typo: unknown configuration key.");
    }

    [Fact]
    public void MaterializeReportsAMissingRequiredSection()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();

        Action materialize = () => services.AddConfixOptions<Mail>(configuration).Materialize();

        materialize.Should().Throw<ConfixValidationException>()
            .Which.Failures.Should().ContainSingle()
            .Which.Should().Be("Mail: required section is missing.");
    }

    [Fact]
    public void AnAbsentOptionalSectionMaterializesTheDeclaredDefaults()
    {
        using var configuration = Config("{}");
        var services = new ServiceCollection();

        var value = services
            .AddConfixOptions<Tuning>(configuration)
            .Materialize();

        value.BatchSize.Should().Be(2);
    }

    [Fact]
    public void MaterializeFailuresDoNotEchoConfiguredValues()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"sensitive-value\",\"Port\":\"oops\"}}");
        var services = new ServiceCollection();

        Action materialize = () => services.AddConfixOptions<Mail>(configuration).Materialize();

        materialize.Should().Throw<ConfixValidationException>()
            .Which.Failures.Should().OnlyContain(e => !e.Contains("sensitive-value") && !e.Contains("oops"));
    }

    [Fact]
    public void TheBuilderStillChainsAsAnOptionsBuilder()
    {
        using var configuration = Config("{\"Mail\":{\"Host\":\"server\"}}");
        var services = new ServiceCollection();

        services.AddConfixOptions<Mail>(configuration).PostConfigure(o => o.Port = 2525);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<Mail>>().Value.Port.Should().Be(2525);
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
        [Required]
        public string Host { get; set; } = "";

        [Range(1, 65535)]
        public int Port { get; set; } = 587;
    }

    [ConfixSection("Tuning", Required = false)]
    public sealed class Tuning
    {
        public int BatchSize { get; set; } = 2;
    }
}
