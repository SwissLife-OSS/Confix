using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Confix;

public static class ConfixOptionsExtensions
{
    public static ConfixOptionsBuilder<T> AddConfixOptions<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? section = null,
        string? name = null,
        bool? required = null)
        where T : class
    {
        var contract = AddContract<T>(
            services, configuration, section, name, required, ConfixEnforcement.Always);

        var builder = new ConfixOptionsBuilder<T>(
            services.AddOptions(),
            contract.Name,
            configuration,
            contract.Section,
            contract.Required);

        builder.Configure(value => Bind(value, configuration, contract.Section, contract.Name));

        services.AddSingleton<IOptionsChangeTokenSource<T>>(
            new ConfigurationChangeTokenSource<T>(contract.Name, configuration));

        if (contract.Required || HasSection(configuration, contract.Section))
        {
            builder.ValidateOnStart();
        }

        return builder;
    }

    /// <summary>
    /// Describes a section that the caller binds itself: its shape is validated and <c>confix</c>
    /// can see it. Use this from libraries that already own their options registration.
    /// <para>
    /// Safe to call unconditionally. The description is inert until the application itself uses
    /// Confix, or until <c>confix validate</c> inspects the application.
    /// </para>
    /// </summary>
    public static IServiceCollection AddConfixSection<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? section = null,
        string? name = null,
        bool? required = null)
        where T : class
    {
        AddContract<T>(services, configuration, section, name, required, ConfixEnforcement.WhenActive);

        return services;
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

        // Libraries claim sections too, so an overlap is reported by validation rather than
        // thrown into the face of an application that does not use Confix.
        try
        {
            EnsureSectionAvailable(services, section, typeof(ConfixSectionClaim), name: null);
        }
        catch (InvalidOperationException ex)
        {
            services.AddSingleton(new ConfixConflict(ex.Message));

            return services;
        }

        services.AddSingleton<IConfixContract>(new Claim(section, required, configuration));

        return services;
    }

    private static Contract<T> AddContract<T>(
        IServiceCollection services,
        IConfiguration configuration,
        string? section,
        string? name,
        bool? required,
        ConfixEnforcement enforcement)
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

        var contract = new Contract<T>(
            section, name, required.Value, configuration, enforcement);

        if (enforcement is ConfixEnforcement.Always)
        {
            EnsureSectionAvailable(services, section, typeof(T), name);
            services.TryAddSingleton(new ConfixApplicationMarker());
        }
        else if (!TryReserveSection(services, contract))
        {
            // A library must never crash an application that does not use Confix, so an
            // overlapping description is reported by validation instead of thrown here.
            return contract;
        }

        // The runner resolves IOptionsMonitor<T>, so the open generics must be present even
        // when the caller owns the binding.
        services.AddOptions();
        services.AddSingleton<IConfixContract>(contract);
        services.AddSingleton<IValidateOptions<T>>(
            sp => new ContractValidator<T>(contract, configuration, sp));

        return contract;
    }

    private static bool TryReserveSection<T>(IServiceCollection services, Contract<T> contract)
        where T : class
    {
        try
        {
            EnsureSectionAvailable(services, contract.Section, typeof(T), contract.Name);

            return true;
        }
        catch (InvalidOperationException ex)
        {
            services.AddSingleton(new ConfixConflict(ex.Message));

            return false;
        }
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
    internal static void Bind<T>(T value, IConfiguration configuration, string section, string name)
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
        IConfiguration Configuration,
        ConfixEnforcement Enforcement) : IConfixContract
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
        Contract<T> contract,
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

            if (contract.Enforcement is ConfixEnforcement.WhenActive &&
                !ConfixActivation.IsActive(services))
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
