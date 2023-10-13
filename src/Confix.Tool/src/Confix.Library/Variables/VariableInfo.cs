namespace Confix.Variables;

public sealed record VariableInfo(
    string ProviderName,
    string ProviderType,
    string VariableName,
    string VariableValue,
    string Path);
