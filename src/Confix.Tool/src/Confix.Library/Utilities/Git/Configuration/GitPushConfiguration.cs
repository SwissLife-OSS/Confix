namespace Confix.Utilities;

public sealed record GitPushConfiguration(
    string Location,
    string[]? Arguments);
