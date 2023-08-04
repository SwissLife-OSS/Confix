using System.Text.Json.Nodes;

namespace Confix.Variables;

public sealed record JsonVariableRewriterContext(
    IReadOnlyDictionary<VariablePath, JsonNode> VariableLookup
);
