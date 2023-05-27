using System.Text.Json.Serialization;

namespace Confix.Tool.Abstractions;

public class ComponentSettingsFile
{
    public ComponentSettingsFile(string name)
    {
        Name = name;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
