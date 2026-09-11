using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Confix;

public static class ConfixOptionsExtensions
{
    public static OptionsBuilder<T> AddConfixOptions<T>(this IServiceCollection services,
        IConfiguration configuration, string? section = null, string? name = null) where T : class
    {
        var attribute = typeof(T).GetCustomAttribute<ConfixSectionAttribute>()
            ?? throw new InvalidOperationException($"{typeof(T).Name} requires ConfixSection.");
        section ??= attribute.Path;
        name ??= Microsoft.Extensions.Options.Options.DefaultName;
        if (section.Length > 0 && section.Split(':').Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("A Confix section must contain nonempty path segments.");
        }
        EnsureUniqueRegistration<T>(services, section, name);
        var contract = new Contract<T>(section, name, attribute.Required, configuration);
        services.AddSingleton<IConfixContract>(contract);
        services.AddSingleton<IValidateOptions<T>>(sp => new ContractValidator<T>(contract, configuration, sp));
        // Avoid binder exception messages that may contain supplied values. Bind inside a sanitized callback.
        var builder = services.AddOptions<T>(name).Configure(value =>
        {
            try
            {
                var source = section.Length == 0 ? configuration : configuration.GetSection(section);
                source.Bind(value);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                throw new ConfixValidationException(name, typeof(T), [$"{section}: configuration binding failed."]);
            }
        });
        services.AddSingleton<IOptionsChangeTokenSource<T>>(new ConfigurationChangeTokenSource<T>(name, configuration));
        if (attribute.Required || HasSection(configuration, section))
        {
            builder.ValidateOnStart();
        }
        // ValidateOnStart accumulates callbacks, so coverage is wired up only for the first contract.
        if (services.All(descriptor => descriptor.ServiceType != typeof(CoverageSource)))
        {
            services.AddSingleton(new CoverageSource(configuration));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<CoverageOptions>, CoverageValidator>());
            services.AddOptions<CoverageOptions>().ValidateOnStart();
        }
        return builder;
    }

    public static IServiceCollection AddConfixModule<T>(this IServiceCollection services,
        IConfiguration configuration) where T : IConfixModule, new()
    {
        new T().Configure(services, configuration);
        return services;
    }

    private static void EnsureUniqueRegistration<T>(IServiceCollection services, string section, string name)
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
            var overlapping = contract.Section.Length == 0 || section.Length == 0 ||
                contract.Section.Equals(section, StringComparison.OrdinalIgnoreCase) ||
                contract.Section.StartsWith(section + ":", StringComparison.OrdinalIgnoreCase) ||
                section.StartsWith(contract.Section + ":", StringComparison.OrdinalIgnoreCase);
            if (overlapping)
            {
                throw new InvalidOperationException(
                    $"Confix section '{Display(section)}' of {typeof(T).Name} overlaps " +
                    $"'{Display(contract.Section)}' of {contract.OptionsType.Name}.");
            }
        }
    }

    private static string Display(string section) => section.Length == 0 ? "(root)" : section;

    private sealed record Contract<T>(string Section, string Name, bool Required, IConfiguration Configuration) : IConfixContract where T : class
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

    private sealed class ContractValidator<T>(IConfixContract contract, IConfiguration configuration, IServiceProvider services) : IValidateOptions<T> where T : class
    {
        public ValidateOptionsResult Validate(string? name, T options)
        {
            if (name != contract.Name)
            {
                return ValidateOptionsResult.Skip;
            }
            var errors = new List<string>();
            var section = contract.Section.Length == 0 ? configuration : configuration.GetSection(contract.Section);
            var present = HasSection(configuration, contract.Section);
            if (!contract.Required && !present)
            {
                return ValidateOptionsResult.Success;
            }
            if (contract.Required && !present)
            {
                errors.Add($"{contract.Section}: required section is missing.");
            }
            ContractValidation.CheckKeys(section, typeof(T), contract.Section, errors);
            ContractValidation.ValidateObject(options, contract.Section, services, errors);
            if (errors.Count > 0)
            {
                throw new ConfixValidationException(name ?? "", typeof(T), errors);
            }
            return ValidateOptionsResult.Success;
        }
    }

    internal static bool HasSection(IConfiguration configuration, string path)
    {
        if (path.Length == 0)
        {
            return true;
        }
        var last = path.LastIndexOf(':');
        var parent = last < 0 ? configuration : configuration.GetSection(path[..last]);
        return parent.GetChildren().Any(c => c.Key.Equals(path[(last + 1)..], StringComparison.OrdinalIgnoreCase));
    }
}
