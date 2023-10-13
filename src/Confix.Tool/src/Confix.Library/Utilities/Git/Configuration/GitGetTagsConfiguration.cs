namespace Confix.Utilities;

public sealed record GitGetTagsConfiguration(
    string Location,
    string[]? Arguments);
