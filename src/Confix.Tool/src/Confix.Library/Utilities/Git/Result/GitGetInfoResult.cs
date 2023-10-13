namespace Confix.Utilities;

public sealed record GitGetRepoInfoResult(
    string Hash,
    string Message,
    string Author,
    string Email);
