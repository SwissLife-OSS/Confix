using System.CommandLine.Builder;
using Factory =
    System.Func<System.IServiceProvider, Confix.Tool.Middlewares.IConfigurationAdapter>;

namespace Confix.Tool.Middlewares;

public static class ConfigurationAdapterCommandBuilderExtensions
{
    private static Context.Key<List<Factory>> _key =
        new("Confix.Tool.Entites.ConfigurationAdapters");

    public static CommandLineBuilder AddConfigurationAdapter<T>(this CommandLineBuilder builder)
        where T : IConfigurationAdapter, new()
    {
        builder.GetConfigurationAdapterLookup().Add(_ => new T());

        return builder;
    }

    private static IList<Factory> GetConfigurationAdapterLookup(this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_key, out var lookup))
        {
            lookup = new List<Factory>();
            contextData.Set(_key, lookup);

            builder.AddSingleton<IEnumerable<IConfigurationAdapter>>(
                sp => lookup.Select(f => f(sp)).ToList());
        }

        return lookup;
    }
}
