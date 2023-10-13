namespace Confix.Utilities;

public sealed record GitSparseCheckoutConfiguration(
    string RepositoryUrl,
    string Location,
    string[]? Arguments);
