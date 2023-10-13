namespace Confix.Tool.Reporting;

public sealed record ComponentReport(
    string ProviderName,
    string ComponentName,
    string? Version,
    IReadOnlyList<string> MountingPoints);
