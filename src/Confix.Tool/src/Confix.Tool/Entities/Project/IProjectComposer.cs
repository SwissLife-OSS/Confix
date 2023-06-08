using Confix.Tool.Abstractions;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public interface IProjectComposer
{
    JsonSchema Compose(IEnumerable<Component> components);
}
