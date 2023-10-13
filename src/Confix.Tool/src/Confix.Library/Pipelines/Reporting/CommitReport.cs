namespace Confix.Tool.Commands.Solution;

public sealed record CommitReport(
    string Hash,
    string Message,
    string Author,
    string Branch,
    IReadOnlyList<string> Tags,
    DateTimeOffset Timestamp);
