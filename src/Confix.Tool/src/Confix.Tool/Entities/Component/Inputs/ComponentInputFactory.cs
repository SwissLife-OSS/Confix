using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode, Confix.Tool.Entities.Components.IComponentInput>;

namespace Confix.Tool.Entities.Components;

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
            throw new ExitException(
                $"Component input type '{definition.Type.AsHighlighted()}' is not registered.")
                {Help = "Check the documentation for a list of supported component inputs."};
        }

        return factory(definition.Value);
    }
}
