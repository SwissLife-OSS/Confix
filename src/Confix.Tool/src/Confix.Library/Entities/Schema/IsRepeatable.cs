using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace Confix.Tool.Schema;

public sealed class IsRepeatable : DirectiveTypeDescriptorAttribute
{
    /// <inheritdoc />
    protected override void OnConfigure(IDescriptorContext context, IDirectiveTypeDescriptor descriptor, Type type)
    {
        descriptor.Repeatable();
    }
}
