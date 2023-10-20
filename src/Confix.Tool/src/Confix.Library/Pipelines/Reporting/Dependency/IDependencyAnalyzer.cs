using System.Text.Json.Nodes;

namespace Confix.Tool.Reporting;

public interface IDependencyAnalyzer
{
    void Analyze(DependencyAnalyzerContext context, JsonNode node);

    bool HasProviders();
}
