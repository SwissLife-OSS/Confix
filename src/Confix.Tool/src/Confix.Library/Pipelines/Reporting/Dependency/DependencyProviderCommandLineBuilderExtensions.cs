using System.Text.Json.Nodes;
using Confix.Tool.Middlewares;
using Confix.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Reporting;

public static class DependencyProviderCommandLineBuilderExtensions
{
    private static Context.Key<Dictionary<string, Factory<IDependencyProvider>>> _key =
        new("Confix.Tool.Entites.Component.DependencyProviders");

    public static ConfixCommandLineBuilder AddDependencyProvider<T>(this ConfixCommandLineBuilder builder)
        where T : IDependencyProvider, new()
        => builder.AddDependencyProvider(T.Type, _ => new T());

    public static ConfixCommandLineBuilder AddDependencyProvider<T>(
        this ConfixCommandLineBuilder builder,
        Func<JsonNode, T> factory)
        where T : IDependencyProvider
        => builder.AddDependencyProvider(T.Type, (_, c) => factory(c));

    public static ConfixCommandLineBuilder AddDependencyProvider<T>(
        this ConfixCommandLineBuilder builder,
        Func<IServiceProvider, JsonNode, T> factory)
        where T : IDependencyProvider
        => builder.AddDependencyProvider(T.Type, (sp, c) => factory(sp, c));

    public static ConfixCommandLineBuilder AddDependencyProvider(
        this ConfixCommandLineBuilder builder,
        string name,
        Func<JsonNode, IDependencyProvider> factory)
    {
        builder.GetDependencyProviderLookup().Add(name, (_, c) => factory(c));

        return builder;
    }

    public static ConfixCommandLineBuilder AddDependencyProvider(
        this ConfixCommandLineBuilder builder,
        string name,
        Factory<IDependencyProvider> factory)
    {
        builder.GetDependencyProviderLookup().Add(name, factory);

        return builder;
    }

    private static Dictionary<string, Factory<IDependencyProvider>> GetDependencyProviderLookup(
        this ConfixCommandLineBuilder builder)
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

    public static ConfixCommandLineBuilder RegisterDependencyProviders(this ConfixCommandLineBuilder builder)
    {
        builder.AddDependencyProvider(c => new RegexDependencyProvider(c));
        builder.AddDependencyProvider<GraphQLDependencyProvider>();

        return builder;
    }
}
