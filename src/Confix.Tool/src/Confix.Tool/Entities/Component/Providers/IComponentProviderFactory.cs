using Confix.Tool.Abstractions;

namespace Confix.Tool.Entities.Components;

public interface IComponentProviderFactory
{
    IComponentProvider CreateProvider(ComponentProviderDefinition definition);
}
