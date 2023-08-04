using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode,
        Confix.Tool.Entities.Components.IComponentProvider>;

namespace Confix.Tool.Entities.Components;

public static class ComponentProviderCommandLineBuilderExtensions
{
    private const string _componentProviders = "Confix.Tool.Entites.Component.ComponentProviders";

    public static CommandLineBuilder AddComponentProvider<T>(this CommandLineBuilder builder)
        where T : IComponentProvider, new()
        => builder.AddComponentProvider(T.Type, _ => new T());

    public static CommandLineBuilder AddComponentProvider(
        this CommandLineBuilder builder,
        string name,
        Func<JsonNode, IComponentProvider> factory)
    {
        builder.GetComponentProviderLookup().Add(name, factory);

        return builder;
    }

    private static Dictionary<string, Factory> GetComponentProviderLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_componentProviders, out Dictionary<string, Factory>? lookup))
        {
            lookup = new Dictionary<string, Factory>();
            contextData.Add(_componentProviders, lookup);

            builder.AddSingleton<IComponentProviderFactory>(_
                => new ComponentProviderFactory(lookup));
        }

        return lookup;
    }

    public static CommandLineBuilder RegisterComponentProviders(this CommandLineBuilder builder)
    {
        builder.AddSingleton(sp
            => new BuildComponentProviderMiddleware(
                sp.GetRequiredService<IComponentProviderFactory>()));

        builder.AddComponentProvider<DotnetPackageComponentProvider>();

        return builder;
    }
}
