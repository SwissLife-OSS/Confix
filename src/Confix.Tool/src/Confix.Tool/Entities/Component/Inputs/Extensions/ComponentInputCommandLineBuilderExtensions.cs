using System.CommandLine.Builder;
using Confix.Tool.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode, Confix.Tool.Entities.Components.IComponentInput>;

namespace Confix.Tool.Entities.Components;

public static class ComponentInputCommandLineBuilderExtensions
{
    private const string _componentInputs = "Confix.Tool.Entites.Component.ComponentInputs";

    public static CommandLineBuilder AddComponentInput<T>(this CommandLineBuilder builder)
        where T : IComponentInput, new()
    {
        builder.GetComponentInputLookup().Add(T.Type, _ => new T());

        return builder;
    }

    private static Dictionary<string, Factory> GetComponentInputLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_componentInputs, out Dictionary<string, Factory>? lookup))
        {
            lookup = new Dictionary<string, Factory>();
            contextData.Add(_componentInputs, lookup);

            builder.AddSingleton<IComponentInputFactory>(_ => new ComponentInputFactory(lookup));
        }

        return lookup;
    }

    public static CommandLineBuilder RegisterComponentInputs(this CommandLineBuilder builder)
    {
        builder.AddSingleton(sp
            => new BuildComponentInputMiddleware(
                sp.GetRequiredService<IComponentInputFactory>()));

        builder.AddComponentInput<GraphQlComponentInput>();

        return builder;
    }
}
