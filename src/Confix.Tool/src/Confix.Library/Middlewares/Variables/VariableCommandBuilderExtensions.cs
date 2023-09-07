using System.Collections.Concurrent;
using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using Confix.Variables;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class VariableCommandBuilderExtensions
{
    private static Context.Key<Dictionary<string, Func<JsonNode, IVariableProvider>>> _key =
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
        builder.AddVariableProvider(config => new GitVariableProvider(config));

        return builder;
    }

    public static CommandLineBuilder AddVariableProvider<T>(
        this CommandLineBuilder builder,
        Func<JsonNode, T> factory)
        where T : IVariableProvider
    {
        builder.GetVariableProviderLookup().Add(T.Type, c => factory(c));

        return builder;
    }

    public static CommandLineBuilder AddVariableProvider<T>(
        this CommandLineBuilder builder,
        string name,
        Func<JsonNode, T> factory)
        where T : IVariableProvider
    {
        builder.GetVariableProviderLookup().Add(name, c => factory(c));

        return builder;
    }

    private static Dictionary<string, Func<JsonNode, IVariableProvider>> GetVariableProviderLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_key, out var lookup))
        {
            lookup = new Dictionary<string, Func<JsonNode, IVariableProvider>>();
            contextData.Set(_key, lookup);

            builder.AddSingleton<IVariableProviderFactory>(_
                => new VariableProviderFactory(lookup));
        }

        return lookup;
    }
}