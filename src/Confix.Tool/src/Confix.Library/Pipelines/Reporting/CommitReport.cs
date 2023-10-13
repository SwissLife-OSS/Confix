namespace Confix.Tool.Reporting;

public sealed record CommitReport(
    string Hash,
    string Message,
    string Author,
    string Email,
    string Branch,
    IReadOnlyList<string> Tags);
