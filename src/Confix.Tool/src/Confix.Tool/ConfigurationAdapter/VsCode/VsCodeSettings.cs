using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Middlewares;

public sealed class VsCodeSettings
{
    private static class FieldNames
    {
        public const string JsonSchemas = "json.schemas";
    }

    public IList<VsCodeJsonSchemas> Schemas { get; set; } = new List<VsCodeJsonSchemas>();

    public static VsCodeSettings From(JsonNode node)
    {
        var jsonSchemas = node
                .MaybeObject()
                ?.MaybeProperty(FieldNames.JsonSchemas)
                ?.ExpectArray()
                .OfType<JsonNode>()
                .Select(VsCodeJsonSchemas.From)
                .ToList()
            ?? new List<VsCodeJsonSchemas>();

        return new VsCodeSettings
        {
            Schemas = jsonSchemas
        };
    }

    public void WriteTo(JsonNode currentSettings)
    {
        currentSettings[FieldNames.JsonSchemas] = JsonSerializer.SerializeToNode(Schemas);
    }
}
