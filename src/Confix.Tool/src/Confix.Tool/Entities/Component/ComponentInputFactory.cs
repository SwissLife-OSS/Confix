using System.Text.Json.Nodes;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode, Confix.Tool.Entities.Component.IComponentInput>;

namespace Confix.Tool.Entities.Component;

public sealed class ComponentInputFactory : IComponentInputFactory
{
    private readonly IReadOnlyDictionary<string, Factory> _lookup;

    public ComponentInputFactory(IReadOnlyDictionary<string, Factory> lookup)
    {
        _lookup = lookup;
    }

    public IComponentInput CreateInput(string type, JsonNode configuration)
    {
        if (!_lookup.TryGetValue(type, out var factory))
        {
            throw new InvalidOperationException(
                $"Component input type '{type}' is not registered.");
        }

        return factory(configuration);
    }
}
