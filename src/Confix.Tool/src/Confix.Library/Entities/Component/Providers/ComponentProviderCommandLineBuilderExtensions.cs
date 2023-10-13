using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Entities.Components.Git;
using Confix.Tool.Middlewares;
using Confix.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Entities.Components;

public static class ComponentProviderCommandLineBuilderExtensions
{
    private static Context.Key<Dictionary<string, Factory<IComponentProvider>>> _key =
        new("Confix.Tool.Entites.Component.ComponentProviders");

    public static CommandLineBuilder AddComponentProvider<T>(this CommandLineBuilder builder)
        where T : IComponentProvider, new()
        => builder.AddComponentProvider(T.Type, _ => new T());

    public static CommandLineBuilder AddComponentProvider<T>(
        this CommandLineBuilder builder,
        Func<JsonNode, T> factory)
        where T : IComponentProvider
        => builder.AddComponentProvider(T.Type, (_, c) => factory(c));

    public static CommandLineBuilder AddComponentProvider<T>(
        this CommandLineBuilder builder,
        Func<IServiceProvider, JsonNode, T> factory)
        where T : IComponentProvider
        => builder.AddComponentProvider(T.Type, (sp, c) => factory(sp, c));

    public static CommandLineBuilder AddComponentProvider(
        this CommandLineBuilder builder,
        string name,
        Func<JsonNode, IComponentProvider> factory)
    {
        builder.GetComponentProviderLookup().Add(name, (_, c) => factory(c));

        return builder;
    }

    public static CommandLineBuilder AddComponentProvider(
        this CommandLineBuilder builder,
        string name,
        Factory<IComponentProvider> factory)
    {
        builder.GetComponentProviderLookup().Add(name, factory);

        return builder;
    }

    private static Dictionary<string, Factory<IComponentProvider>> GetComponentProviderLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_key, out var lookup))
        {
            lookup = new Dictionary<string, Factory<IComponentProvider>>();
            contextData.Set(_key, lookup);

            builder.AddSingleton<IComponentProviderFactory>(
                sp => new ComponentProviderFactory(sp, lookup));
        }

        return lookup;
    }

    public static CommandLineBuilder RegisterComponentProviders(this CommandLineBuilder builder)
    {
        builder.AddSingleton(sp
            => new BuildComponentProviderMiddleware(
                sp.GetRequiredService<IComponentProviderFactory>()));

        builder.AddComponentProvider<DotnetPackageComponentProvider>();
        builder.AddComponentProvider((sp, c)
            => new GitComponentProvider(sp.GetRequiredService<IGitService>(), c));

        return builder;
    }
}
