
using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using ConfiX.Variables;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares;

public static class VariableCommandBuilderExtensions
{
    private const string _variableProviders = "Confix.Tool.Entites.Variables.VariableProvider";

    public static CommandLineBuilder RegisterVariableMiddleware(this CommandLineBuilder builder)
    {
        builder.AddDefaultVariableProviders();
        builder.AddSingleton(sp => new VariableMiddleware(sp.GetRequiredService<IVariableProviderFactory>()));

        return builder;
    }
    public static CommandLineBuilder AddDefaultVariableProviders(this CommandLineBuilder builder)
    {
        builder.AddVariableProvider("local", (config) => new LocalVariableProvider(config));

        return builder;
    }

    public static CommandLineBuilder AddVariableProvider(
            this CommandLineBuilder builder,
            string name,
            Func<JsonNode, IVariableProvider> creator)
    {
        builder.GetVariableProviderLookup().Add(name, creator);

        return builder;
    }

    private static Dictionary<string, Func<JsonNode, IVariableProvider>> GetVariableProviderLookup(
       this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_variableProviders, out Dictionary<string, Func<JsonNode, IVariableProvider>>? lookup))
        {
            lookup = new Dictionary<string, Func<JsonNode, IVariableProvider>>();
            contextData.Add(_variableProviders, lookup);

            builder.AddSingleton(_ => new VariableProviderFactory(lookup));
        }

        return lookup;
    }
}