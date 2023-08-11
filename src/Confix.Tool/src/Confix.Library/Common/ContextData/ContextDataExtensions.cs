using System.Diagnostics.CodeAnalysis;

namespace Confix.Tool;

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

    public static bool TryGetValue<T>(
        this IDictionary<string, object> contextData,
        in Context.Key<T> key,
        [NotNullWhen(true)] out T? value)
    {
        if (contextData.TryGetValue(key.Id, out var v))
        {
            value = (T) v;
            return true;
        }

        value = default;
        return false;
    }

    public static T GetOrAddValue<T>(
        this IDictionary<string, object> contextData,
        in Context.Key<T> key) where T : new()
    {
        if (!contextData.TryGetValue(key.Id, out var v) || v is not T value)
        {
            value = new T();
            contextData[key.Id] = value;
        }

        return value;
    }
}
