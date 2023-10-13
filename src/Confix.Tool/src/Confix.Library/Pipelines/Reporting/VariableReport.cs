namespace Confix.Tool.Reporting;

public sealed record VariableReport(
    string VariableName,
    string ProviderName,
    string ProviderType,
    string Hash,
    string Path);
