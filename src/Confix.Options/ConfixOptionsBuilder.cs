using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix;

/// <summary>
/// The builder returned by <c>AddConfixOptions</c>. Registration alone configures the options
/// pipeline; <see cref="Materialize"/> additionally binds and validates the value immediately,
/// for the rare composition-time decision that cannot wait for the container.
/// </summary>
public sealed class ConfixOptionsBuilder<T> : OptionsBuilder<T>
    where T : class
{
    private readonly IConfiguration _configuration;
    private readonly string _section;
    private readonly bool _required;

    internal ConfixOptionsBuilder(
        IServiceCollection services,
        string name,
        IConfiguration configuration,
        string section,
        bool required)
        : base(services, name)
    {
        _configuration = configuration;
        _section = section;
        _required = required;
    }

    /// <summary>
    /// Binds the section now and runs the contract's shape and declared-rule validation, so an
    /// early read is never less safe than resolving <c>IOptions&lt;T&gt;</c>. Options pipeline
    /// callbacks such as <c>PostConfigure</c> are not applied; an absent optional section
    /// yields the type's declared defaults.
    /// </summary>
    public T Materialize()
    {
        if (!ConfixOptionsExtensions.HasSection(_configuration, _section))
        {
            if (_required)
            {
                throw new ConfixValidationException(
                    Name,
                    typeof(T),
                    [$"{_section}: required section is missing."]);
            }

            return Activator.CreateInstance<T>();
        }

        var value = Activator.CreateInstance<T>();

        ConfixOptionsExtensions.Bind(value, _configuration, _section, Name);

        var section = _section.Length == 0 ? _configuration : _configuration.GetSection(_section);
        var errors = new List<string>();

        ContractValidation.CheckKeys(section, typeof(T), _section, errors);
        ContractValidation.ValidateObject(value, _section, NullServiceProvider.Instance, errors);

        if (errors.Count > 0)
        {
            throw new ConfixValidationException(Name, typeof(T), errors);
        }

        return value;
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public static readonly NullServiceProvider Instance = new();

        public object? GetService(Type serviceType) => null;
    }
}
