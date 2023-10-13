namespace Confix.Utilities;

public sealed record GitCommitConfiguration(
    string Location,
    string Message,
    string[]? Arguments);
