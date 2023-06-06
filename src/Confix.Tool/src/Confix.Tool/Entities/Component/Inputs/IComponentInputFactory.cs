using Confix.Tool.Abstractions;

namespace Confix.Tool.Entities.Component;

public interface IComponentInputFactory
{
    IComponentInput CreateInput(ComponentInputDefinition configuration);
}
