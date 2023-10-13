namespace Confix.Tool.Commands.Solution;

public sealed record Report(
    string ConfigurationPath,
    string Environment,
    DateTimeOffset Timestamp,
    ProjectReport Project,
    SolutionReport? Solution,
    RepositoryReport Repository,
    CommitReport Commit);
