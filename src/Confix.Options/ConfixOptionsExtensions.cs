using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Confix;

public static class ConfixOptionsExtensions
{
    public static OptionsBuilder<T> AddConfixOptions<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? section = null,
        string? name = null,
        bool? required = null)
        where T : class
    {
        var attribute = typeof(T).GetCustomAttribute<ConfixSectionAttribute>();

        // Types owned by another package cannot be annotated, so the caller mounts them instead.
        if (attribute is null && section is null)
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} has no ConfixSection attribute, so a section must be supplied.");
        }

        section ??= attribute!.Path;
        name ??= Options.DefaultName;
        required ??= attribute?.Required ?? true;

        EnsureValidSection(section);
        EnsureSectionAvailable(services, section, typeof(T), name);

        var contract = new Contract<T>(section, name, required.Value, configuration);

        services.AddSingleton<IConfixContract>(contract);
        services.AddSingleton<IValidateOptions<T>>(
            sp => new ContractValidator<T>(contract, configuration, sp));

        var builder = services.AddOptions<T>(name)
            .Configure(value => Bind(value, configuration, section, name));

        services.AddSingleton<IOptionsChangeTokenSource<T>>(
            new ConfigurationChangeTokenSource<T>(name, configuration));

        if (required.Value || HasSection(configuration, section))
        {
            builder.ValidateOnStart();
        }

        AddCoverageValidation(services, configuration);

        return builder;
    }

    /// <summary>
    /// Claims a section that another component owns: it counts towards coverage and is checked
    /// for presence, but its contents are validated by whoever defines them.
    /// </summary>
    public static IServiceCollection AddConfixSection(
        this IServiceCollection services,
        IConfiguration configuration,
        string section,
        bool required = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(section);

        EnsureValidSection(section);
        EnsureSectionAvailable(services, section, typeof(ConfixSectionClaim), name: null);

        services.AddSingleton<IConfixContract>(new Claim(section, required, configuration));
        AddCoverageValidation(services, configuration);

        return services;
    }

    private static void EnsureValidSection(string section)
    {
        if (section.Length > 0 && section.Split(':').Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "A Confix section must contain nonempty path segments.");
        }
    }

    // Binder exception messages can contain supplied values, so they never reach the caller.
    private static void Bind<T>(T value, IConfiguration configuration, string section, string name)
        where T : class
    {
        try
        {
            var source = section.Length == 0 ? configuration : configuration.GetSection(section);
            source.Bind(value);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            throw new ConfixValidationException(
                name,
                typeof(T),
                [$"{section}: configuration binding failed."]);
        }
    }

    // ValidateOnStart accumulates callbacks, so coverage is wired up only for the first contract.
    private static void AddCoverageValidation(
        IServiceCollection services,
        IConfiguration configuration)
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(CoverageSource)))
        {
            return;
        }

        services.AddSingleton(new CoverageSource(configuration));
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<CoverageOptions>, CoverageValidator>());
        services.AddOptions<CoverageOptions>().ValidateOnStart();
    }

    private static void EnsureSectionAvailable(
        IServiceCollection services,
        string section,
        Type optionsType,
        string? name)
    {
        var contracts = services
            .Where(descriptor => descriptor.ServiceType == typeof(IConfixContract))
            .Select(descriptor => descriptor.ImplementationInstance)
            .OfType<IConfixContract>();

        foreach (var contract in contracts)
        {
            if (name is not null && contract.OptionsType == optionsType && contract.Name == name)
            {
                throw new InvalidOperationException(
                    $"{optionsType.Name} is already registered under the name '{name}'.");
            }

            if (contract.Section.Equals(section, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Section '{ContractValidation.Display(section)}' is already claimed by " +
                    $"{contract.OptionsType.Name}.");
            }

            if (IsNestedIn(section, contract.Section))
            {
                throw new InvalidOperationException(ContractValidation.NestingError(
                    contract.Section, contract.OptionsType, section, optionsType));
            }

            if (IsNestedIn(contract.Section, section))
            {
                throw new InvalidOperationException(ContractValidation.NestingError(
                    section, optionsType, contract.Section, contract.OptionsType));
            }
        }
    }

    private static bool IsNestedIn(string section, string parent)
    {
        return parent.Length == 0
            ? section.Length > 0
            : section.StartsWith(parent + ":", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool HasSection(IConfiguration configuration, string path)
    {
        if (path.Length == 0)
        {
            return true;
        }

        var last = path.LastIndexOf(':');
        var parent = last < 0 ? configuration : configuration.GetSection(path[..last]);
        var key = path[(last + 1)..];

        return parent.GetChildren().Any(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record Contract<T>(
        string Section,
        string Name,
        bool Required,
        IConfiguration Configuration) : IConfixContract
        where T : class
    {
        public Type OptionsType => typeof(T);

        public void Validate(IServiceProvider services)
        {
            if (!Required && !HasSection(Configuration, Section))
            {
                return;
            }

            _ = services.GetRequiredService<IOptionsMonitor<T>>().Get(Name);
        }
    }

    /// <summary>Owns a section for coverage without inspecting what it contains.</summary>
    private sealed record Claim(
        string Section,
        bool Required,
        IConfiguration Configuration) : IConfixContract
    {
        public Type OptionsType => typeof(ConfixSectionClaim);

        public string Name => Options.DefaultName;

        public void Validate(IServiceProvider services)
        {
            if (Required && !HasSection(Configuration, Section))
            {
                throw new ConfixValidationException(
                    Name,
                    typeof(ConfixSectionClaim),
                    [$"{Section}: required section is missing."]);
            }
        }
    }

    private sealed class ContractValidator<T>(
        IConfixContract contract,
        IConfiguration configuration,
        IServiceProvider services) : IValidateOptions<T>
        where T : class
    {
        public ValidateOptionsResult Validate(string? name, T options)
        {
            if (name != contract.Name)
            {
                return ValidateOptionsResult.Skip;
            }

            var present = HasSection(configuration, contract.Section);

            if (!contract.Required && !present)
            {
                return ValidateOptionsResult.Success;
            }

            var errors = new List<string>();

            if (!present)
            {
                errors.Add($"{contract.Section}: required section is missing.");
            }

            var section = contract.Section.Length == 0
                ? configuration
                : configuration.GetSection(contract.Section);

            ContractValidation.CheckKeys(section, typeof(T), contract.Section, errors);
            ContractValidation.ValidateObject(options, contract.Section, services, errors);

            if (errors.Count > 0)
            {
                throw new ConfixValidationException(name ?? string.Empty, typeof(T), errors);
            }

            return ValidateOptionsResult.Success;
        }
    }
}
