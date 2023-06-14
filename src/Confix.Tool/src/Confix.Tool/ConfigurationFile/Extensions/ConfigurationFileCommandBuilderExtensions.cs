using System.CommandLine.Builder;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode,
        Confix.Tool.Middlewares.IConfigurationFileProvider>;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFileCommandBuilderExtensions
{
    private const string _componentFiles = "Confix.Tool.Entites.Configuration.ConfigurationFiles";

    public static CommandLineBuilder AddConfigurationFileProvider<T>(
        this CommandLineBuilder builder)
        where T : IConfigurationFileProvider, new()
    {
        builder.GetConfigurationFileProviderLookup().Add(T.Type, _ => new T());

        return builder;
    }

    private static Dictionary<string, Factory> GetConfigurationFileProviderLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_componentFiles, out Dictionary<string, Factory>? lookup))
        {
            lookup = new Dictionary<string, Factory>();
            contextData.Add(_componentFiles, lookup);

            builder.AddSingleton<IConfigurationFileProviderFactory>(_
                => new ConfigurationFileProviderFactory(lookup));
        }

        return lookup;
    }
}
