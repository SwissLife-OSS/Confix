namespace Confix.Extensions;

public static class Context
{
    public static Key<bool> DisableStatus { get; } = new(nameof(DisableStatus));

    public readonly record struct Key<T>(string Id);
}
