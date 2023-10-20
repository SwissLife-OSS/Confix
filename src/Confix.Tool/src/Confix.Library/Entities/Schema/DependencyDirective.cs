using HotChocolate.Types;

namespace Confix.Tool.Schema;

[DirectiveType(Name, DirectiveLocation.InputFieldDefinition | DirectiveLocation.FieldDefinition)]
[IsRepeatable]
public sealed class DependencyDirective
{
    internal const string Name = "dependency";

    public string Kind { get; set; } = default!;
}