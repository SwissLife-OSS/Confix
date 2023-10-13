using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using Confix.Utilities;
using Confix.Variables;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class VariableCommandBuilderExtensions
{
    private static Context.Key<Dictionary<string, Factory<IVariableProvider>>> _key =
        new("Confix.Tool.Entites.Variables.VariableProvider");

    public static CommandLineBuilder RegisterVariableMiddleware(this CommandLineBuilder builder)
    {
        builder.AddDefaultVariableProviders();
        builder.AddSingleton<VariableListCache>();
        builder.AddTransient(sp
            => new VariableMiddleware(sp.GetRequiredService<IVariableProviderFactory>()));

        return builder;
    }

    private static CommandLineBuilder AddDefaultVariableProviders(this CommandLineBuilder builder)
    {
        builder.AddVariableProvider(config => new LocalVariableProvider(config));
        builder.AddVariableProvider(config => new SecretVariableProvider(config));
        builder.AddVariableProvider(config => new AzureKeyVaultProvider(config));
        builder.AddVariableProvider((sp, config)
            => new GitVariableProvider(sp.GetRequiredService<IGitService>(), config));

        return builder;
    }

    public static CommandLineBuilder AddVariableProvider<T>(
        this CommandLineBuilder builder,
        Func<IServiceProvider, JsonNode, T> factory)
        where T : IVariableProvider
    {
        builder.GetVariableProviderLookup().Add(T.Type, (sp, c) => factory(sp, c));

        return builder;
    }

    public static CommandLineBuilder AddVariableProvider<T>(
        this CommandLineBuilder builder,
        Func<JsonNode, T> factory)
        where T : IVariableProvider
    {
        builder.GetVariableProviderLookup().Add(T.Type, (_, c) => factory(c));

        return builder;
    }

    public static CommandLineBuilder AddVariableProvider<T>(
        this CommandLineBuilder builder,
        string name,
        Func<JsonNode, T> factory)
        where T : IVariableProvider
    {
        builder.GetVariableProviderLookup().Add(name, (_, c) => factory(c));

        return builder;
    }

    public static CommandLineBuilder AddVariableProvider<T>(
        this CommandLineBuilder builder,
        string name,
        Factory<T> factory)
        where T : IVariableProvider
    {
        builder.GetVariableProviderLookup().Add(name, (sp, c) => factory(sp, c));

        return builder;
    }

    private static Dictionary<string, Factory<IVariableProvider>> GetVariableProviderLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_key, out var lookup))
        {
            lookup = new Dictionary<string, Factory<IVariableProvider>>();
            contextData.Set(_key, lookup);

            builder.AddSingleton<IVariableProviderFactory>(
                sp => new VariableProviderFactory(sp, lookup));
        }

        return lookup;
    }
}
