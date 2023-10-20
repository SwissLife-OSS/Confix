using System.Text.Json;
using HotChocolate;
using HotChocolate.Types;
using static HotChocolate.Types.DirectiveLocation;

namespace Confix.Tool.Schema;

[DirectiveType(Name, InputFieldDefinition | FieldDefinition)]
public sealed class DefaultValueDirective
{
    internal const string Name = "defaultValue";

    [GraphQLType(typeof(NonNullType<JsonType>))]
    public JsonElement Value { get; set; }
}
