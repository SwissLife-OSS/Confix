namespace Confix.Utilities;

public sealed record GitPullConfiguration(
    string Location,
    string[]? Arguments);
