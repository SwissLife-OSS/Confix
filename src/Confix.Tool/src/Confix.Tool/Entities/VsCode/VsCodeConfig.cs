using System.Text.Json.Serialization;

namespace Confix.Tool.Entities.VsCode;

public class VsCodeConfig
{
    [JsonPropertyName("json.schemas")]
    public List<JsonSchemas> JsonSchemas { get; set; } = new();
}

public class JsonSchemas
{
    [JsonPropertyName("fileMatch")]
    public List<string> FileMatch { get; set; } = new();

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
