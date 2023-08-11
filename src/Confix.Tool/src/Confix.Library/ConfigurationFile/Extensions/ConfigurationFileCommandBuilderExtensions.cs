using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode,
        Confix.Tool.Middlewares.IConfigurationFileProvider>;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFileCommandBuilderExtensions
{
    private static Context.Key<Dictionary<string, Factory>> _key =
        new("Confix.Tool.Entites.Configuration.ConfigurationFiles");

    public static CommandLineBuilder AddConfigurationFileProvider<T>(
        this CommandLineBuilder builder)
        where T : IConfigurationFileProvider, new()
        => builder.AddConfigurationFileProvider(T.Type, _ => new T());

    public static CommandLineBuilder AddConfigurationFileProvider(
        this CommandLineBuilder builder,
        string name,
        Func<JsonNode, IConfigurationFileProvider> factory)
    {
        builder.GetConfigurationFileProviderLookup().Add(name, factory);

        return builder;
    }

    private static Dictionary<string, Factory> GetConfigurationFileProviderLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_key, out var lookup))
        {
            lookup = new Dictionary<string, Factory>();
            contextData.Set(_key, lookup);

            builder.AddSingleton<IConfigurationFileProviderFactory>(_
                => new ConfigurationFileProviderFactory(lookup));
        }

        return lookup;
    }
}
