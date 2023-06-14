using Confix.Tool.Abstractions;
using ConfiX.Variables;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public interface IProjectComposer
{
    JsonSchema Compose(IEnumerable<Component> components, IEnumerable<VariablePath> variables);
}
