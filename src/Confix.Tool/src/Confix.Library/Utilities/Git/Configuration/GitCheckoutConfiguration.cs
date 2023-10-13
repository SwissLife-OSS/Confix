namespace Confix.Utilities;

public sealed record GitCheckoutConfiguration(
    string Location,
    string Ref,
    string[]? Arguments);
