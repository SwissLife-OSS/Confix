namespace Confix.Utilities;

public sealed record GitGetRepoInfoConfiguration(
    string Hash,
    string Message,
    string Author,
    string Email);
