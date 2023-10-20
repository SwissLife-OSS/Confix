using System.Text.Json.Nodes;

namespace Confix.Tool.Reporting;

public interface IDependencyProvider
{
    public static virtual string Type => string.Empty;

    void Analyze(DependencyAnalyzerContext context, JsonNode node);
}
