namespace Confix.Extensions;

public static class ContextDataExtensions
{
    public static T? Get<T>(this IDictionary<string, object> contextData, in Context.Key<T> key)
    {
        if (contextData.TryGetValue(key.Id, out var value))
        {
            return (T?) value;
        }

        return default;
    }

    public static void Set<T>(
        this IDictionary<string, object> contextData,
        in Context.Key<T> key,
        T value)
        where T : notnull
    {
        contextData[key.Id] = value;
    }
}
