namespace Confix.Tool.Reporting;

public sealed record Report(
    string ConfigurationPath,
    string Environment,
    DateTimeOffset Timestamp,
    ProjectReport Project,
    SolutionReport? Solution,
    RepositoryReport Repository,
    CommitReport Commit,
    IReadOnlyList<VariableReport> Variables);
