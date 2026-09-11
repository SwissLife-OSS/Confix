using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Confix;

public interface IConfixModule
{
    void Configure(IServiceCollection services, IConfiguration configuration);
}

public interface IConfixContract
{
    Type OptionsType { get; }
    string Section { get; }
    string Name { get; }
    bool Required { get; }
    void Validate(IServiceProvider services);
}

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
            throw new InvalidOperationException("A Confix section must contain nonempty path segments.");
        if (services.Any(d => d.ServiceType == typeof(IConfixContract) &&
            d.ImplementationInstance is IConfixContract c &&
            ((c.OptionsType == typeof(T) && c.Name == name) || c.Section.Equals(section, StringComparison.OrdinalIgnoreCase) || c.Section.Length == 0 || section.Length == 0 ||
                c.Section.StartsWith(section + ":", StringComparison.OrdinalIgnoreCase) || section.StartsWith(c.Section + ":", StringComparison.OrdinalIgnoreCase))))
            throw new InvalidOperationException($"Duplicate Confix registration for {section}.");
        var contract = new Contract<T>(section, name, attribute.Required, configuration);
        services.AddSingleton<IConfixContract>(contract);
        services.AddSingleton<IValidateOptions<T>>(sp => new ContractValidator<T>(contract, configuration, sp));
        // Avoid binder exception messages that may contain supplied values. Bind inside a sanitized callback.
        var builder = services.AddOptions<T>(name).Configure(value =>
        {
            try { (section.Length == 0 ? configuration : configuration.GetSection(section)).Bind(value); }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            { throw new ConfixValidationException(name, typeof(T), [$"{section}: configuration binding failed."]); }
        });
        services.AddSingleton<IOptionsChangeTokenSource<T>>(new ConfigurationChangeTokenSource<T>(name, configuration));
        if (attribute.Required || HasSection(configuration, section)) builder.ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<CoverageOptions>, CoverageValidator>());
        services.TryAddSingleton(new CoverageSource(configuration));
        services.AddOptions<CoverageOptions>().ValidateOnStart();
        return builder;
    }

    public static IServiceCollection AddConfixModule<T>(this IServiceCollection services,
        IConfiguration configuration) where T : IConfixModule, new()
    {
        new T().Configure(services, configuration);
        return services;
    }

    private sealed record Contract<T>(string Section, string Name, bool Required, IConfiguration Configuration) : IConfixContract where T : class
    {
        public Type OptionsType => typeof(T);
        public void Validate(IServiceProvider services)
        {
            if (!Required && !HasSection(Configuration, Section)) return;
            _ = services.GetRequiredService<IOptionsMonitor<T>>().Get(Name);
        }
    }

    private sealed class ContractValidator<T>(IConfixContract contract, IConfiguration configuration, IServiceProvider services) : IValidateOptions<T> where T : class
    {
        public ValidateOptionsResult Validate(string? name, T options)
        {
            if (name != contract.Name) return ValidateOptionsResult.Skip;
            var errors = new List<string>();
            var section = contract.Section.Length == 0 ? configuration : configuration.GetSection(contract.Section);
            if (!contract.Required && !HasSection(configuration, contract.Section)) return ValidateOptionsResult.Success;
            if (contract.Required && !HasSection(configuration, contract.Section))
                errors.Add($"{contract.Section}: required section is missing.");
            ContractValidation.CheckKeys(section, typeof(T), contract.Section, errors);
            ContractValidation.ValidateObject(options, contract.Section, services, errors);
            if (errors.Count > 0) throw new ConfixValidationException(name ?? "", typeof(T), errors);
            return ValidateOptionsResult.Success;
        }
    }

    internal static bool HasSection(IConfiguration configuration, string path)
    {
        if (path.Length == 0) return true;
        var last = path.LastIndexOf(':');
        var parent = last < 0 ? configuration : configuration.GetSection(path[..last]);
        return parent.GetChildren().Any(c => c.Key.Equals(path[(last + 1)..], StringComparison.OrdinalIgnoreCase));
    }
}
