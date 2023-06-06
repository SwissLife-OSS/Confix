using Confix.Tool.Abstractions;
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

    public IComponentInput CreateInput(ComponentInputDefinition definition)
    {
        if (!_lookup.TryGetValue(definition.Type, out var factory))
        {
            throw new InvalidOperationException(
                $"Component input type '{definition.Type}' is not registered.");
        }

        return factory(definition.Value);
    }
}
