using System.CommandLine.Builder;
using Factory =
    System.Func<System.IServiceProvider, Confix.Tool.Middlewares.IConfigurationAdapter>;

namespace Confix.Tool.Middlewares;

public static class ConfigurationAdapterCommandBuilderExtensions
{
    private const string _configurationAdpaters = "Confix.Tool.Entites.ConfigurationAdapters";

    public static CommandLineBuilder AddConfigurationAdapter<T>(this CommandLineBuilder builder)
        where T : IConfigurationAdapter, new()
    {
        builder.GetConfigurationAdapterLookup().Add(_ => new T());

        return builder;
    }

    private static IList<Factory> GetConfigurationAdapterLookup(this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_configurationAdpaters, out List<Factory>? lookup))
        {
            lookup = new List<Factory>();
            contextData.Add(_configurationAdpaters, lookup);

            builder.AddSingleton<IEnumerable<IConfigurationAdapter>>(
                sp => lookup.Select(f => f(sp)).ToList());
        }

        return lookup;
    }
}
