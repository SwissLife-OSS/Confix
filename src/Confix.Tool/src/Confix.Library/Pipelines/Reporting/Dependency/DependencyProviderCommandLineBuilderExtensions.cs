using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using Confix.Tool.Middlewares;
using Confix.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Reporting;

public static class DependencyProviderCommandLineBuilderExtensions
{
    private static Context.Key<Dictionary<string, Factory<IDependencyProvider>>> _key =
        new("Confix.Tool.Entites.Component.DependencyProviders");

    public static CommandLineBuilder AddDependencyProvider<T>(this CommandLineBuilder builder)
        where T : IDependencyProvider, new()
        => builder.AddDependencyProvider(T.Type, _ => new T());

    public static CommandLineBuilder AddDependencyProvider<T>(
        this CommandLineBuilder builder,
        Func<JsonNode, T> factory)
        where T : IDependencyProvider
        => builder.AddDependencyProvider(T.Type, (_, c) => factory(c));

    public static CommandLineBuilder AddDependencyProvider<T>(
        this CommandLineBuilder builder,
        Func<IServiceProvider, JsonNode, T> factory)
        where T : IDependencyProvider
        => builder.AddDependencyProvider(T.Type, (sp, c) => factory(sp, c));

    public static CommandLineBuilder AddDependencyProvider(
        this CommandLineBuilder builder,
        string name,
        Func<JsonNode, IDependencyProvider> factory)
    {
        builder.GetDependencyProviderLookup().Add(name, (_, c) => factory(c));

        return builder;
    }

    public static CommandLineBuilder AddDependencyProvider(
        this CommandLineBuilder builder,
        string name,
        Factory<IDependencyProvider> factory)
    {
        builder.GetDependencyProviderLookup().Add(name, factory);

        return builder;
    }

    private static Dictionary<string, Factory<IDependencyProvider>> GetDependencyProviderLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_key, out var lookup))
        {
            lookup = new Dictionary<string, Factory<IDependencyProvider>>();
            contextData.Set(_key, lookup);

            builder.AddSingleton<IDependencyProviderFactory>(
                sp => new DependencyProviderFactory(sp, lookup));
        }

        return lookup;
    }

    public static CommandLineBuilder RegisterDependencyProviders(this CommandLineBuilder builder)
    {
        builder.AddDependencyProvider(c => new RegexDependencyProvider(c));
        builder.AddDependencyProvider<GraphQLDependencyProvider>();

        return builder;
    }
}
