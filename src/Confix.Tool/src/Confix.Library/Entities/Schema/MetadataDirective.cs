using System.Text.Json;
using HotChocolate;
using HotChocolate.Types;

namespace Confix.Tool.Schema;

[DirectiveType(Name, DirectiveLocation.InputFieldDefinition | DirectiveLocation.FieldDefinition)]
[IsRepeatable]
public sealed class MetadataDirective
{
    internal const string Name = "metadata";

    [GraphQLType("JSON!")]
    public JsonElement Value { get; set; }
}
