using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Confix;

public sealed class ConfixValidationSettings
{
    public bool StrictCoverage { get; set; } = true;

    /// <summary>
    /// Explicit application source boundary for startup coverage. Defaults to JSON providers.
    /// </summary>
    public IConfiguration? Sources { get; set; }
}

internal sealed class CoverageOptions;

internal sealed record CoverageSource(IConfiguration Configuration);

/// <summary>
/// At startup check owned JSON paths, without treating ambient environment keys as appsettings.
/// </summary>
internal sealed class CoverageValidator(IServiceProvider services, CoverageSource source)
    : IValidateOptions<CoverageOptions>
{
    private const string JsonProviderNamespace = "Microsoft.Extensions.Configuration.Json";

    public ValidateOptionsResult Validate(string? name, CoverageOptions options)
    {
        var settings = services.GetService<IOptions<ConfixValidationSettings>>()?.Value ?? new();

        if (!settings.StrictCoverage)
        {
            return ValidateOptionsResult.Success;
        }

        if (settings.Sources is { } selected)
        {
            return ValidateCoverage(selected);
        }

        if (source.Configuration is not IConfigurationRoot root)
        {
            return ValidateOptionsResult.Fail(
                "Confix startup coverage needs an IConfigurationRoot with explicit JSON sources.");
        }

        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in root.Providers)
        {
            if (provider.GetType().Namespace == JsonProviderNamespace)
            {
                Collect(provider, null, data);
            }
        }

        using var configuration = (ConfigurationRoot)new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();

        return ValidateCoverage(configuration);
    }

    private ValidateOptionsResult ValidateCoverage(IConfiguration configuration)
    {
        var contracts = services.GetServices<IConfixContract>().ToArray();
        var errors = new List<string>();

        if (!contracts.Any(contract => contract.Section.Length == 0))
        {
            ContractValidation.CheckCoverage(configuration, string.Empty, contracts, errors);
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private static void Collect(
        IConfigurationProvider provider,
        string? parent,
        Dictionary<string, string?> data)
    {
        foreach (var key in provider.GetChildKeys([], parent).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var path = parent is null ? key : parent + ":" + key;

            if (provider.TryGet(path, out var value))
            {
                data[path] = value;
            }

            Collect(provider, path, data);
        }
    }
}
