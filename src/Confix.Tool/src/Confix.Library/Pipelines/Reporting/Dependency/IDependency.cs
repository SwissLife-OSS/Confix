using System.Text.Json;

namespace Confix.Tool.Reporting;

public interface IDependency
{
    public string Type { get; }
    
    public string Kind { get; }

    public string Path { get; }

    void WriteTo(Utf8JsonWriter writer);
}
