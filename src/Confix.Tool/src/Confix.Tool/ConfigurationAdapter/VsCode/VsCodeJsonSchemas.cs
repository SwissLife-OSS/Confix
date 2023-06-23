using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Confix.Utilities.Json;

namespace Confix.Tool.Middlewares;

public class VsCodeJsonSchemas
{
    private static class FieldNames
    {
        public const string FileMatch = "fileMatch";
        public const string Url = "url";
        public const string ProjectPath = "projectPath";
        public const string ProjectName = "projectName";
    }

    [JsonPropertyName(FieldNames.FileMatch)]
    public IList<string> FileMatch { get; set; } = new List<string>();

    [JsonPropertyName(FieldNames.Url)]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName(FieldNames.ProjectPath)]
    public string? ProjectPath { get; set; } = string.Empty;

    [JsonPropertyName(FieldNames.ProjectName)]
    public string? ProjectName { get; set; } = string.Empty;

    public static VsCodeJsonSchemas From(JsonNode node)
    {
        var obj = node.ExpectObject();
        var fileMatch = obj
            .ExpectProperty(FieldNames.FileMatch)
            .ExpectArray()
            .Select(x => x.ExpectValue<string>())
            .ToList();

        var url = obj.ExpectProperty(FieldNames.Url).ExpectValue<string>();
        var projectPath = obj.MaybeProperty(FieldNames.ProjectPath)?.ExpectValue<string>();
        var projectName = obj.MaybeProperty(FieldNames.ProjectName)?.ExpectValue<string>();

        return new VsCodeJsonSchemas
        {
            FileMatch = fileMatch,
            Url = url,
            ProjectPath = projectPath,
            ProjectName = projectName
        };
    }
}