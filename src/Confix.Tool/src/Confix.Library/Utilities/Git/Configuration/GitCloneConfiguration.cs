namespace Confix.Utilities;

public sealed record GitCloneConfiguration(
    string RepositoryUrl,
    string Location,
    string SparseDirectory,
    string[]? Arguments);
