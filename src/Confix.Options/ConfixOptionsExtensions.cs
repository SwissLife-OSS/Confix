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
        string? name = null)
        where T : class
    {
        var attribute = typeof(T).GetCustomAttribute<ConfixSectionAttribute>()
            ?? throw new InvalidOperationException($"{typeof(T).Name} requires ConfixSection.");

        section ??= attribute.Path;
        name ??= Options.DefaultName;

        if (section.Length > 0 && section.Split(':').Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "A Confix section must contain nonempty path segments.");
        }

        EnsureUniqueRegistration<T>(services, section, name);

        var contract = new Contract<T>(section, name, attribute.Required, configuration);

        services.AddSingleton<IConfixContract>(contract);
        services.AddSingleton<IValidateOptions<T>>(
            sp => new ContractValidator<T>(contract, configuration, sp));

        var builder = services.AddOptions<T>(name)
            .Configure(value => Bind(value, configuration, section, name));

        services.AddSingleton<IOptionsChangeTokenSource<T>>(
            new ConfigurationChangeTokenSource<T>(name, configuration));

        if (attribute.Required || HasSection(configuration, section))
        {
            builder.ValidateOnStart();
        }

        AddCoverageValidation(services, configuration);

        return builder;
    }

    public static IServiceCollection AddConfixModule<T>(
        this IServiceCollection services,
        IConfiguration configuration)
        where T : IConfixModule, new()
    {
        new T().Configure(services, configuration);

        return services;
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

    private static void EnsureUniqueRegistration<T>(
        IServiceCollection services,
        string section,
        string name)
    {
        var contracts = services
            .Where(descriptor => descriptor.ServiceType == typeof(IConfixContract))
            .Select(descriptor => descriptor.ImplementationInstance)
            .OfType<IConfixContract>();

        foreach (var contract in contracts)
        {
            if (contract.OptionsType == typeof(T) && contract.Name == name)
            {
                throw new InvalidOperationException(
                    $"{typeof(T).Name} is already registered under the name '{name}'.");
            }

            if (contract.Section.Equals(section, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Section '{ContractValidation.Display(section)}' is already claimed by " +
                    $"{contract.OptionsType.Name}.");
            }

            // Nesting is legal in either registration order when the parent cannot bind the key.
            var conflict = IsNestedIn(section, contract.Section)
                ? ContractValidation.NestingConflict(
                    contract.Section, contract.OptionsType, section, typeof(T))
                : IsNestedIn(contract.Section, section)
                    ? ContractValidation.NestingConflict(
                        section, typeof(T), contract.Section, contract.OptionsType)
                    : null;

            if (conflict is not null)
            {
                throw new InvalidOperationException(conflict);
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

            var delegated = ContractValidation.DelegatedPaths(
                services.GetServices<IConfixContract>(), contract);

            ContractValidation.CheckKeys(section, typeof(T), contract.Section, errors, delegated);
            ContractValidation.ValidateObject(options, contract.Section, services, errors);

            if (errors.Count > 0)
            {
                throw new ConfixValidationException(name ?? string.Empty, typeof(T), errors);
            }

            return ValidateOptionsResult.Success;
        }
    }
}
