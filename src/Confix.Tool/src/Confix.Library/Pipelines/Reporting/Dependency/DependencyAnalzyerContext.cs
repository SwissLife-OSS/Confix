using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;
using Json.Schema;

namespace Confix.Tool.Reporting;

public sealed class DependencyAnalyzerContext
{
    private readonly List<IDependency> _dependencies = new();
    private readonly JsonSchema? _schema;
    private readonly Dictionary<string, object?> _contextData = new();

    public DependencyAnalyzerContext(JsonSchema? schema, JsonNode document)
    {
        _schema = schema;
        Document = document;
    }

    public JsonNode Document { get; }

    public JsonSchema GetSchema(string analyzer)
    {
        if (_schema is null)
        {
            throw new ExitException(
                $"The analyzer {analyzer} requires the confix project to be restored first.")
            {
                Help = $"Run {"confix restore".AsCommand()} first!"
            };
        }

        return _schema;
    }

    public bool TryGetContextData<T>(string key, out T? value)
    {
        if (_contextData.TryGetValue(key, out var obj) && obj is T objOfT)
        {
            value = objOfT;
            return true;
        }

        value = default;
        return false;
    }

    public void SetContextData<T>(string key, T value)
    {
        _contextData[key] = value;
    }

    public void AddDependency(IDependency dependency)
    {
        _dependencies.Add(dependency);
    }

    public IEnumerable<IDependency> GetDependencies()
    {
        return _dependencies;
    }
}
