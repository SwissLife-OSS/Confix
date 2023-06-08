using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public sealed record JsonVariableRewriterContext(
    IReadOnlyDictionary<VariablePath, JsonValue> VariableLookup
);
