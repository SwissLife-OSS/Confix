using Confix.Tool.Abstractions;

namespace Confix.Tool.Entities.Components;

public interface IComponentInputFactory
{
    IComponentInput CreateInput(ComponentInputDefinition definition);
}
