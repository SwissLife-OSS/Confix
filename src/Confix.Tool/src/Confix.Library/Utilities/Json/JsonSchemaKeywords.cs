namespace Confix.Tool.Schema;

/// <summary>
/// The names of the JSON schema keywords that are used by Confix. JsonSchema.Net no longer
/// exposes keyword types since version 9.
/// </summary>
public static class JsonSchemaKeywords
{
    public const string Ref = "$ref";

    public const string Defs = "$defs";

    public const string Type = "type";

    public const string AnyOf = "anyOf";

    public const string Properties = "properties";

    public const string Required = "required";
}
