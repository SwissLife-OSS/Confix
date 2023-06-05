using System.CommandLine.Builder;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode, Confix.Tool.Entities.Component.IComponentInput>;

namespace Confix.Tool.Entities.Component;

public static class ComponentInputCommandBuilderExtensions
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

            builder.AddSingleton(_ => new ComponentInputFactory(lookup));
        }

        return lookup;
    }
}
